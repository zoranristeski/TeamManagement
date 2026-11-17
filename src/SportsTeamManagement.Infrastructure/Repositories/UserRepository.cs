using Microsoft.EntityFrameworkCore;
using SportsTeamManagement.Application.Common.Interfaces;
using SportsTeamManagement.Domain.Entities;
using SportsTeamManagement.Infrastructure.Data;

namespace SportsTeamManagement.Infrastructure.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByAuth0IdAsync(string auth0Id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(u => !u.IsDeleted)
                .FirstOrDefaultAsync(u => u.Auth0Id == auth0Id, cancellationToken);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(u => !u.IsDeleted)
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        }

        public async Task<User?> GetUserWithStudentAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(u => !u.IsDeleted)
                .Include(u => u.Student)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        }

        public async Task<User?> GetUserWithTrainerAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(u => !u.IsDeleted)
                .Include(u => u.Trainer)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        }
    }
}