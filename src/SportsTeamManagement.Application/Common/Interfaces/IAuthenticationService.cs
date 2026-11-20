using SportsTeamManagement.Application.DTOs;
using SportsTeamManagement.Application.Common.Models;

namespace SportsTeamManagement.Application.Common.Interfaces;

public interface IAuthenticationService
{
    Task<Result<AuthResponseDto>> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);
    Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<Result<AuthResponseDto>> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<Result> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> GetMeAsync(Guid userId, CancellationToken cancellationToken = default);
}
