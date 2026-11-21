using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SportsTeamManagement.Application.Common.Interfaces;
using SportsTeamManagement.Application.Common.Models;
using SportsTeamManagement.Application.DTOs;
using SportsTeamManagement.Domain.Entities;
using SportsTeamManagement.Domain.Enums;
using SportsTeamManagement.Infrastructure.Data;

namespace SportsTeamManagement.Infrastructure.Services;

public sealed class AuthenticationService : IAuthenticationService
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public AuthenticationService(ApplicationDbContext db, IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _db = db;
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient("AuthenticationProviders");
    }

    public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
            return Result.Failure<AuthResponseDto>("First name and last name are required.");
        if (!request.AcceptTerms)
            return Result.Failure<AuthResponseDto>("Terms must be accepted.");

        var email = request.Email.Trim();
        var normalizedEmail = email.ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(email) || email.Length > 255 || !email.Contains('@'))
            return Result.Failure<AuthResponseDto>("A valid email address is required.");
        if (!IsStrongPassword(request.Password))
            return Result.Failure<AuthResponseDto>("Password must be at least 12 characters and include upper, lower, number, and symbol characters.");

        if (await _db.Users.AnyAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken))
            return Result.Failure<AuthResponseDto>("Registration could not be completed.");

        var user = new User
        {
            Auth0Id = $"local:{Guid.NewGuid()}",
            Email = email,
            NormalizedEmail = normalizedEmail,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            PasswordHash = PasswordHashing.Hash(request.Password),
            EmailVerified = false,
            Role = UserRole.Student,
            IsActive = true
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);
        return await IssueAsync(user, cancellationToken);
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToUpperInvariant();
        var user = await _db.Users.SingleOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken);
        if (user is null || !user.IsActive || string.IsNullOrWhiteSpace(user.PasswordHash))
            return Result.Failure<AuthResponseDto>("Invalid email or password.");
        if (user.LockoutEndAt.HasValue && user.LockoutEndAt.Value > DateTime.UtcNow)
            return Result.Failure<AuthResponseDto>("The account is temporarily locked.");

        if (!PasswordHashing.Verify(request.Password, user.PasswordHash))
        {
            user.AccessFailedCount++;
            if (user.AccessFailedCount >= 5)
            {
                user.LockoutEndAt = DateTime.UtcNow.AddMinutes(15);
                user.AccessFailedCount = 0;
            }
            await _db.SaveChangesAsync(cancellationToken);
            return Result.Failure<AuthResponseDto>("Invalid email or password.");
        }

        user.AccessFailedCount = 0;
        user.LockoutEndAt = null;
        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return await IssueAsync(user, cancellationToken);
    }

    public async Task<Result<AuthResponseDto>> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var hash = HashToken(refreshToken);
        var stored = await _db.RefreshTokens
            .Include(x => x.User)
            .SingleOrDefaultAsync(x => x.TokenHash == hash, cancellationToken);
        if (stored is null)
            return Result.Failure<AuthResponseDto>("The refresh token is invalid.");

        if (stored.RevokedAt.HasValue)
        {
            await RevokeFamilyAsync(stored.TokenFamilyId, "Refresh token reuse detected", cancellationToken);
            return Result.Failure<AuthResponseDto>("The refresh token is invalid.");
        }
        if (stored.ExpiresAt <= DateTime.UtcNow)
            return Result.Failure<AuthResponseDto>("The refresh token is expired.");

        stored.RevokedAt = DateTime.UtcNow;
        stored.RevocationReason = "Rotated";
        await _db.SaveChangesAsync(cancellationToken);
        return await IssueAsync(stored.User, cancellationToken, stored.TokenFamilyId);
    }

    public async Task<Result> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return Result.Success();
        var hash = HashToken(refreshToken);
        var stored = await _db.RefreshTokens.SingleOrDefaultAsync(x => x.TokenHash == hash, cancellationToken);
        if (stored is not null)
        {
            stored.RevokedAt = DateTime.UtcNow;
            stored.RevocationReason = "Logout";
            await _db.SaveChangesAsync(cancellationToken);
        }
        return Result.Success();
    }

    public async Task<Result<UserDto>> GetMeAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users.SingleOrDefaultAsync(x => x.Id == userId, cancellationToken);
        return user is null ? Result.Failure<UserDto>("User not found.") : Result.Success(ToDto(user));
    }

    public async Task<Result<List<string>>> GetProvidersAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var providers = await _db.ExternalLogins
            .Where(x => x.UserId == userId)
            .Select(x => x.Provider)
            .Distinct()
            .ToListAsync(cancellationToken);
        return Result.Success(providers);
    }

    public async Task<Result<AuthResponseDto>> ExternalLoginAsync(string provider, ExternalLoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var profile = await ValidateProviderAsync(provider, request, cancellationToken);
        if (profile is null)
            return Result.Failure<AuthResponseDto>("The external login could not be verified.");

        var existing = await _db.ExternalLogins.Include(x => x.User)
            .SingleOrDefaultAsync(x => x.Provider == profile.Provider && x.ProviderUserId == profile.ProviderUserId, cancellationToken);
        if (existing is not null)
        {
            existing.Email = profile.Email;
            existing.EmailVerified = profile.EmailVerified;
            existing.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            return await IssueAsync(existing.User, cancellationToken);
        }

        var email = profile.EmailVerified ? profile.Email : null;
        var normalizedEmail = email is null ? null : email.Trim().ToUpperInvariant();
        if (!await _db.Users.AnyAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken) && normalizedEmail is not null)
        {
            // Matching email is accepted only for an explicit linking operation; external-first
            // registration always creates a new identity and requires authenticated linking later.
        }

        var localEmail = normalizedEmail is null
            ? $"{profile.Provider.ToLowerInvariant()}-{profile.ProviderUserId}@external.invalid"
            : normalizedEmail;
        if (await _db.Users.AnyAsync(x => x.NormalizedEmail == localEmail, cancellationToken))
            localEmail = $"{profile.Provider.ToLowerInvariant()}-{Guid.NewGuid():N}@external.invalid";

        var user = new User
        {
            Auth0Id = $"{profile.Provider}:{profile.ProviderUserId}",
            Email = localEmail,
            NormalizedEmail = localEmail,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            EmailVerified = profile.EmailVerified,
            Role = UserRole.Student,
            IsActive = true
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        _db.ExternalLogins.Add(new ExternalLogin
        {
            UserId = user.Id,
            Provider = profile.Provider,
            ProviderUserId = profile.ProviderUserId,
            Email = profile.Email,
            EmailVerified = profile.EmailVerified
        });
        await _db.SaveChangesAsync(cancellationToken);
        return await IssueAsync(user, cancellationToken);
    }

    public async Task<Result> LinkProviderAsync(Guid userId, string provider, ExternalLoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var profile = await ValidateProviderAsync(provider, request, cancellationToken);
        if (profile is null)
            return Result.Failure("The external login could not be verified.");
        if (await _db.ExternalLogins.AnyAsync(x => x.Provider == profile.Provider && x.ProviderUserId == profile.ProviderUserId, cancellationToken))
            return Result.Failure("This external login is already associated with an application account.");

        var user = await _db.Users.SingleOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user is null)
            return Result.Failure("User not found.");

        _db.ExternalLogins.Add(new ExternalLogin
        {
            UserId = userId,
            Provider = profile.Provider,
            ProviderUserId = profile.ProviderUserId,
            Email = profile.Email,
            EmailVerified = profile.EmailVerified
        });
        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> UnlinkProviderAsync(Guid userId, string provider, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users.SingleOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user is null)
            return Result.Failure("User not found.");

        var login = await _db.ExternalLogins.SingleOrDefaultAsync(x => x.UserId == userId && x.Provider == provider, cancellationToken);
        if (login is null)
            return Result.Failure("The provider is not connected.");

        var providerCount = await _db.ExternalLogins.CountAsync(x => x.UserId == userId, cancellationToken);
        if (string.IsNullOrWhiteSpace(user.PasswordHash) && providerCount <= 1)
            return Result.Failure("At least one authentication method must remain.");

        _db.ExternalLogins.Remove(login);
        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private async Task<ProviderProfile?> ValidateProviderAsync(string provider, ExternalLoginRequestDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.RedirectUri))
            return null;

        if (provider.Equals("google", StringComparison.OrdinalIgnoreCase))
            return await ValidateGoogleAsync(request, cancellationToken);
        if (provider.Equals("facebook", StringComparison.OrdinalIgnoreCase))
            return await ValidateFacebookAsync(request, cancellationToken);
        return null;
    }

    private async Task<ProviderProfile?> ValidateGoogleAsync(ExternalLoginRequestDto request, CancellationToken cancellationToken)
    {
        var clientId = _configuration["Authentication:Google:ClientId"];
        var clientSecret = _configuration["Authentication:Google:ClientSecret"];
        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
            return null;

        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["code"] = request.Code,
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
            ["redirect_uri"] = request.RedirectUri,
            ["grant_type"] = "authorization_code"
        });
        using var tokenResponse = await _httpClient.PostAsync("https://oauth2.googleapis.com/token", content, cancellationToken);
        if (!tokenResponse.IsSuccessStatusCode)
            return null;
        var token = await tokenResponse.Content.ReadFromJsonAsync<GoogleTokenResponse>(cancellationToken: cancellationToken);
        if (string.IsNullOrWhiteSpace(token?.id_token))
            return null;

        using var verifyResponse = await _httpClient.GetAsync($"https://oauth2.googleapis.com/tokeninfo?id_token={Uri.EscapeDataString(token.id_token)}", cancellationToken);
        if (!verifyResponse.IsSuccessStatusCode)
            return null;
        var profile = await verifyResponse.Content.ReadFromJsonAsync<GoogleProfileResponse>(cancellationToken: cancellationToken);
        if (profile is null || profile.sub is null || !string.Equals(profile.aud, clientId, StringComparison.Ordinal))
            return null;

        return new ProviderProfile("google", profile.sub, profile.email, profile.email_verified, profile.given_name ?? "External", profile.family_name ?? "User");
    }

    private async Task<ProviderProfile?> ValidateFacebookAsync(ExternalLoginRequestDto request, CancellationToken cancellationToken)
    {
        var appId = _configuration["Authentication:Facebook:AppId"];
        var appSecret = _configuration["Authentication:Facebook:AppSecret"];
        if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(appSecret))
            return null;

        var tokenQuery = $"code={Uri.EscapeDataString(request.Code)}&client_id={Uri.EscapeDataString(appId)}&client_secret={Uri.EscapeDataString(appSecret)}&redirect_uri={Uri.EscapeDataString(request.RedirectUri)}";
        using var tokenResponse = await _httpClient.GetAsync($"https://graph.facebook.com/v20.0/oauth/access_token?{tokenQuery}", cancellationToken);
        if (!tokenResponse.IsSuccessStatusCode)
            return null;
        var token = await tokenResponse.Content.ReadFromJsonAsync<FacebookTokenResponse>(cancellationToken: cancellationToken);
        if (string.IsNullOrWhiteSpace(token?.access_token))
            return null;

        using var debugResponse = await _httpClient.GetAsync($"https://graph.facebook.com/debug_token?input_token={Uri.EscapeDataString(token.access_token)}&access_token={Uri.EscapeDataString(appId)}|{Uri.EscapeDataString(appSecret)}", cancellationToken);
        if (!debugResponse.IsSuccessStatusCode)
            return null;
        var debug = await debugResponse.Content.ReadFromJsonAsync<FacebookDebugResponse>(cancellationToken: cancellationToken);
        if (debug?.data is null || !debug.data.is_valid || debug.data.app_id != appId)
            return null;

        using var profileResponse = await _httpClient.GetAsync($"https://graph.facebook.com/v20.0/me?fields=id,first_name,last_name,email&access_token={Uri.EscapeDataString(token.access_token)}", cancellationToken);
        if (!profileResponse.IsSuccessStatusCode)
            return null;
        var profile = await profileResponse.Content.ReadFromJsonAsync<FacebookProfileResponse>(cancellationToken: cancellationToken);
        if (profile is null || string.IsNullOrWhiteSpace(profile.id))
            return null;

        return new ProviderProfile("facebook", profile.id, profile.email, !string.IsNullOrWhiteSpace(profile.email), profile.first_name ?? "External", profile.last_name ?? "User");
    }

    private async Task<Result<AuthResponseDto>> IssueAsync(User user, CancellationToken cancellationToken, Guid? familyId = null)
    {
        var signingKey = _configuration["Authentication:SigningKey"];
        var issuer = _configuration["Authentication:LocalIssuer"] ?? "SportsTeamManagement";
        var audience = _configuration["Authentication:Audience"] ?? "sports-team-management";
        if (string.IsNullOrWhiteSpace(signingKey) || signingKey.Length < 32)
            return Result.Failure<AuthResponseDto>("Local authentication signing is not configured.");

        var expires = DateTime.UtcNow.AddMinutes(15);
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("jti", Guid.NewGuid().ToString())
        };
        if (user.Role == UserRole.PlatformAdmin)
        {
            var roleClaimType = _configuration["Authentication:RoleClaimType"] ?? "roles";
            claims.Add(new Claim(roleClaimType, nameof(UserRole.PlatformAdmin)));
        }
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(issuer, audience, claims, expires: expires, signingCredentials: credentials);
        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        var refreshToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var storedRefresh = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = HashToken(refreshToken),
            TokenFamilyId = familyId ?? Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };
        _db.RefreshTokens.Add(storedRefresh);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAtUtc = expires,
            User = ToDto(user)
        });
    }

    private async Task RevokeFamilyAsync(Guid familyId, string reason, CancellationToken cancellationToken)
    {
        var family = await _db.RefreshTokens.Where(x => x.TokenFamilyId == familyId && x.RevokedAt == null).ToListAsync(cancellationToken);
        foreach (var token in family)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.RevocationReason = reason;
        }
        await _db.SaveChangesAsync(cancellationToken);
    }

    private static bool IsStrongPassword(string password)
    {
        return password.Length >= 12
            && password.Any(char.IsUpper)
            && password.Any(char.IsLower)
            && password.Any(char.IsDigit)
            && password.Any(c => !char.IsLetterOrDigit(c));
    }

    private static string HashToken(string token)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();
    }

    private static UserDto ToDto(User user) => new()
    {
        Id = user.Id,
        Auth0Id = user.Auth0Id,
        Email = user.Email,
        FirstName = user.FirstName,
        LastName = user.LastName,
        PhoneNumber = user.PhoneNumber,
        Role = user.Role,
        IsActive = user.IsActive,
        ProfileImageUrl = user.ProfileImageUrl,
        FullName = user.FullName,
        CreatedAt = user.CreatedAt
    };

    private sealed record ProviderProfile(string Provider, string ProviderUserId, string? Email, bool EmailVerified, string FirstName, string LastName);
    private sealed record GoogleTokenResponse(string? id_token);
    private sealed record GoogleProfileResponse(string? sub, string? email, bool email_verified, string? given_name, string? family_name, string? aud);
    private sealed record FacebookTokenResponse(string? access_token);
    private sealed record FacebookDebugResponse(FacebookDebugData? data);
    private sealed record FacebookDebugData(bool is_valid, string? app_id);
    private sealed record FacebookProfileResponse(string? id, string? email, string? first_name, string? last_name);
}
