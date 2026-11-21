using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SportsTeamManagement.API.HealthChecks;
using SportsTeamManagement.Application;
using SportsTeamManagement.Infrastructure;
using SportsTeamManagement.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using SportsTeamManagement.Domain.Enums;
using SportsTeamManagement.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Set the database connection using ConnectionStrings__DefaultConnection or local development user secrets.");
}

var authority = builder.Configuration["Authentication:Authority"];
var signingKey = builder.Configuration["Authentication:SigningKey"];
Uri? authorityUri = null;
var useLocalJwt = !string.IsNullOrWhiteSpace(signingKey);
if (!useLocalJwt && (!Uri.TryCreate(authority, UriKind.Absolute, out authorityUri) ||
    authorityUri.Scheme != Uri.UriSchemeHttps))
{
    throw new InvalidOperationException("Authentication:Authority must be an HTTPS issuer URL.");
}

var audience = builder.Configuration["Authentication:Audience"];
if (string.IsNullOrWhiteSpace(audience))
{
    throw new InvalidOperationException("Authentication:Audience must be configured.");
}

var roleClaimType = builder.Configuration["Authentication:RoleClaimType"] ?? "roles";
if (string.IsNullOrWhiteSpace(roleClaimType))
{
    throw new InvalidOperationException("Authentication:RoleClaimType cannot be empty.");
}

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
var seedDevelopmentData = builder.Configuration.GetValue<bool>("SeedData:Enabled");
if (seedDevelopmentData && !builder.Environment.IsDevelopment())
{
    throw new InvalidOperationException("SeedData:Enabled can only be used in the Development environment.");
}

foreach (var origin in allowedOrigins)
{
    if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri) ||
        (uri.Scheme != Uri.UriSchemeHttps &&
         !((builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing")) &&
           uri.Scheme == Uri.UriSchemeHttp &&
           uri.IsLoopback)) ||
        uri.AbsolutePath != "/" ||
        !string.IsNullOrEmpty(uri.Query) ||
        !string.IsNullOrEmpty(uri.Fragment) ||
        !string.IsNullOrEmpty(uri.UserInfo))
    {
        throw new InvalidOperationException(
            "Cors:AllowedOrigins must contain only explicit HTTPS origins (HTTP loopback is allowed in Development and Testing).");
    }
}

if (!builder.Environment.IsDevelopment() && builder.Configuration["PORT"] is { Length: > 0 } portValue)
{
    if (!int.TryParse(portValue, out var port) || port is < 1 or > 65535)
    {
        throw new InvalidOperationException("The PORT environment variable must be a valid TCP port.");
    }

    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

if (!builder.Environment.IsDevelopment())
{
    builder.Logging.ClearProviders();
    builder.Logging.AddJsonConsole();
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.ForwardLimit = 1;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        if (allowedOrigins.Length == 0)
        {
            policy.SetIsOriginAllowed(_ => false);
        }
        else
        {
            policy.WithOrigins(allowedOrigins);
        }

        policy.WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
            .WithHeaders("Authorization", "Content-Type", "X-Club-Id");
    });
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        if (useLocalJwt)
        {
            options.Authority = null;
            options.RequireHttpsMetadata = false;
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = builder.Configuration["Authentication:LocalIssuer"] ?? "SportsTeamManagement",
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey!)),
                RoleClaimType = roleClaimType,
                NameClaimType = "name"
            };
        }
        else
        {
            options.Authority = authorityUri!.ToString();
            options.Audience = audience;
            options.RequireHttpsMetadata = true;
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                RoleClaimType = roleClaimType,
                NameClaimType = "name"
            };
        }
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PlatformAdminOnly", policy => policy.RequireRole("PlatformAdmin"));
    options.AddPolicy("ClubManager", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.Requirements.Add(new ClubRoleRequirement(UserRole.ClubAdmin, UserRole.Trainer, UserRole.Admin));
    });
    options.AddPolicy("ClubTrainer", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.Requirements.Add(new ClubRoleRequirement(UserRole.Trainer, UserRole.ClubAdmin, UserRole.Admin));
    });
    options.AddPolicy("ClubStudent", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.Requirements.Add(new ClubRoleRequirement(UserRole.Student));
    });
    options.AddPolicy("ClubParent", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.Requirements.Add(new ClubRoleRequirement(UserRole.Parent));
    });
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});
builder.Services.AddScoped<IAuthorizationHandler, ClubRoleHandler>();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        if (context.Request.Path.StartsWithSegments("/health"))
        {
            return RateLimitPartition.GetNoLimiter("health");
        }

        var partitionKey = context.User.FindFirstValue("sub")
            ?? context.Connection.RemoteIpAddress?.ToString()
            ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 120,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        });
    });
});

builder.Services.AddHealthChecks()
    .AddCheck<SqlServerHealthCheck>("sql");

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();
var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("UnhandledExceptions");

if (seedDevelopmentData)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var seedLogger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
        .CreateLogger("DevelopmentDataSeeder");
    await DevelopmentDataSeeder.SeedAsync(dbContext, seedLogger);
}

app.UseForwardedHeaders();
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        if (exception is not null)
        {
            logger.LogError(exception, "Unhandled API exception. TraceId: {TraceId}", context.TraceIdentifier);
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";
        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred."
        };
        problem.Extensions["traceId"] = context.TraceIdentifier;
        await context.Response.WriteAsJsonAsync(problem);
    });
});

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = WriteHealthStatus
}).AllowAnonymous();

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = WriteHealthStatus
}).AllowAnonymous();

app.MapControllers();
app.Run();

static Task WriteHealthStatus(HttpContext context, HealthReport report)
{
    context.Response.ContentType = "application/json";
    return context.Response.WriteAsJsonAsync(new { status = report.Status.ToString() });
}

public partial class Program { }
