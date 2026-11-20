using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
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

    public AuthenticationService(ApplicationDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
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
            PasswordHash = HashPassword(request.Password),
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

        if (!VerifyPassword(request.Password, user.PasswordHash))
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

    private async Task<Result<AuthResponseDto>> IssueAsync(User user, CancellationToken cancellationToken, Guid? familyId = null)
    {
        var signingKey = _configuration["Authentication:SigningKey"];
        var issuer = _configuration["Authentication:LocalIssuer"] ?? "SportsTeamManagement";
        var audience = _configuration["Authentication:Audience"] ?? "sports-team-management";
        if (string.IsNullOrWhiteSpace(signingKey) || signingKey.Length < 32)
            return Result.Failure<AuthResponseDto>("Local authentication signing is not configured.");

        var expires = DateTime.UtcNow.AddMinutes(15);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("jti", Guid.NewGuid().ToString())
        };
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

    private static string HashPassword(string password)
    {
        const int iterations = 100_000;
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, 32);
        return $"PBKDF2${iterations}${Convert.ToHexString(salt)}${Convert.ToHexString(hash)}";
    }

    private static bool VerifyPassword(string password, string stored)
    {
        var parts = stored.Split('$');
        if (parts.Length != 4 || !int.TryParse(parts[1], out var iterations))
            return false;
        try
        {
            var salt = Convert.FromHexString(parts[2]);
            var expected = Convert.FromHexString(parts[3]);
            var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException)
        {
            return false;
        }
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
}
