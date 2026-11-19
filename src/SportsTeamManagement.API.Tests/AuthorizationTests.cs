using System.Data.Common;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace SportsTeamManagement.API.Tests;

public sealed class AuthorizationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthorizationTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task LivenessEndpointAllowsAnonymousRequests()
    {
        var response = await _client.GetAsync("/health/live");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task StudentDataEndpointRejectsAnonymousRequests()
    {
        var response = await _client.GetAsync("/api/students/eligible-for-grading");

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task StudentRoleCannotAccessAdminOnlyStudentEndpoints()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/students/eligible-for-grading");
        request.Headers.Add(TestAuthenticationHandler.RoleHeader, "Student");

        var response = await _client.SendAsync(request);

        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task StudentRoleCannotManageClubs()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/clubs");
        request.Headers.Add(TestAuthenticationHandler.RoleHeader, "Student");

        var response = await _client.SendAsync(request);

        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task StudentRoleCannotManageTrainingGroups()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/traininggroups?clubId=00000000-0000-0000-0000-000000000001");
        request.Headers.Add(TestAuthenticationHandler.RoleHeader, "Student");

        var response = await _client.SendAsync(request);

        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CorsAllowsOnlyConfiguredOrigins()
    {
        using var allowedRequest = CreatePreflightRequest("https://frontend.example");
        using var blockedRequest = CreatePreflightRequest("https://untrusted.example");

        var allowedResponse = await _client.SendAsync(allowedRequest);
        var blockedResponse = await _client.SendAsync(blockedRequest);

        Assert.Contains("https://frontend.example", allowedResponse.Headers.GetValues("Access-Control-Allow-Origin"));
        Assert.False(blockedResponse.Headers.Contains("Access-Control-Allow-Origin"));
    }

    private static HttpRequestMessage CreatePreflightRequest(string origin)
    {
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/students/eligible-for-grading");
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", "GET");
        return request;
    }
}

public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly Dictionary<string, string?> _originalEnvironmentValues = new();

    public TestWebApplicationFactory()
    {
        var testConnection = new DbConnectionStringBuilder
        {
            ["Data Source"] = "unused",
            ["Initial Catalog"] = "unused"
        }.ConnectionString;

        SetEnvironmentVariable("ConnectionStrings__DefaultConnection", testConnection);
        SetEnvironmentVariable("Authentication__Authority", "https://issuer.example/");
        SetEnvironmentVariable("Authentication__Audience", "sports-team-management-tests");
        SetEnvironmentVariable("Cors__AllowedOrigins__0", "https://frontend.example");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services =>
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthenticationHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthenticationHandler.SchemeName;
                options.DefaultForbidScheme = TestAuthenticationHandler.SchemeName;
            }).AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                TestAuthenticationHandler.SchemeName,
                _ => { });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            foreach (var (name, value) in _originalEnvironmentValues)
            {
                Environment.SetEnvironmentVariable(name, value);
            }
        }
    }

    private void SetEnvironmentVariable(string name, string value)
    {
        _originalEnvironmentValues[name] = Environment.GetEnvironmentVariable(name);
        Environment.SetEnvironmentVariable(name, value);
    }
}

public sealed class TestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Test";
    public const string RoleHeader = "X-Test-Role";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(RoleHeader, out var role))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new[]
        {
            new Claim("sub", "test-user"),
            new Claim("roles", role.ToString())
        };
        var identity = new ClaimsIdentity(claims, SchemeName, ClaimTypes.Name, "roles");
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
