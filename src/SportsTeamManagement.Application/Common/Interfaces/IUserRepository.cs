using SportsTeamManagement.Domain.Entities;

namespace SportsTeamManagement.Application.Common.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByAuth0IdAsync(string auth0Id, CancellationToken cancellationToken = default);
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<User?> GetUserWithStudentAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<User?> GetUserWithTrainerAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
