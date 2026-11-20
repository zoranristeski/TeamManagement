using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsTeamManagement.Application.Common.Interfaces;
using SportsTeamManagement.Application.DTOs;

namespace SportsTeamManagement.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : BaseApiController
{
    private readonly IAuthenticationService _authentication;

    public AuthController(IAuthenticationService authentication)
    {
        _authentication = authentication;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request, CancellationToken cancellationToken)
        => HandleResult(await _authentication.RegisterAsync(request, cancellationToken));

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
        => HandleResult(await _authentication.LoginAsync(request, cancellationToken));

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto request, CancellationToken cancellationToken)
        => HandleResult(await _authentication.RefreshAsync(request.RefreshToken, cancellationToken));

    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshRequestDto request, CancellationToken cancellationToken)
        => HandleResult(await _authentication.LogoutAsync(request.RefreshToken, cancellationToken));

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var subject = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out var userId))
            return Unauthorized();
        return HandleResult(await _authentication.GetMeAsync(userId, cancellationToken));
    }

    [AllowAnonymous]
    [HttpPost("external/google")]
    public async Task<IActionResult> Google([FromBody] ExternalLoginRequestDto request, CancellationToken cancellationToken)
        => HandleResult(await _authentication.ExternalLoginAsync("google", request, cancellationToken));

    [AllowAnonymous]
    [HttpPost("external/facebook")]
    public async Task<IActionResult> Facebook([FromBody] ExternalLoginRequestDto request, CancellationToken cancellationToken)
        => HandleResult(await _authentication.ExternalLoginAsync("facebook", request, cancellationToken));

    [Authorize]
    [HttpGet("providers")]
    public async Task<IActionResult> Providers(CancellationToken cancellationToken)
    {
        var subject = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(subject, out var userId)
            ? HandleResult(await _authentication.GetProvidersAsync(userId, cancellationToken))
            : Unauthorized();
    }

    [Authorize]
    [HttpPost("link-provider/{provider}")]
    public async Task<IActionResult> LinkProvider(string provider, [FromBody] ExternalLoginRequestDto request, CancellationToken cancellationToken)
    {
        var subject = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(subject, out var userId)
            ? HandleResult(await _authentication.LinkProviderAsync(userId, provider, request, cancellationToken))
            : Unauthorized();
    }

    [Authorize]
    [HttpDelete("unlink-provider/{provider}")]
    public async Task<IActionResult> UnlinkProvider(string provider, CancellationToken cancellationToken)
    {
        var subject = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(subject, out var userId)
            ? HandleResult(await _authentication.UnlinkProviderAsync(userId, provider, cancellationToken))
            : Unauthorized();
    }
}
