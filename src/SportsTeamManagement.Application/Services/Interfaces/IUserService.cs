using SportsTeamManagement.Application.Common.Models;
using SportsTeamManagement.Application.DTOs;

namespace SportsTeamManagement.Application.Services.Interfaces
{
    public interface IUserService
    {
        Task<Result<UserDto>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<Result<UserDto>> GetUserByAuth0IdAsync(string auth0Id, CancellationToken cancellationToken = default);
        Task<Result<UserDto>> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<Result<UserDto>> CreateUserAsync(CreateUserDto createUserDto, CancellationToken cancellationToken = default);
        Task<Result<UserDto>> UpdateUserAsync(Guid userId, UpdateUserDto updateUserDto, CancellationToken cancellationToken = default);
        Task<Result> DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<Result<IEnumerable<UserDto>>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    }
}
