using Microsoft.EntityFrameworkCore;
using SportsTeamManagement.Application.Common.Interfaces;
using SportsTeamManagement.Domain.Entities;
using SportsTeamManagement.Domain.Enums;
using SportsTeamManagement.Infrastructure.Data;

namespace SportsTeamManagement.Infrastructure.Repositories
{
    public class StudentRepository : Repository<Student>, IStudentRepository
    {
        public StudentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Student>> GetStudentsByStatusAsync(StudentStatus status, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(s => !s.IsDeleted)
                .Include(s => s.User)
                .Where(s => s.Status == status)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Student>> GetStudentsByBeltRankAsync(BeltRank beltRank, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(s => !s.IsDeleted)
                .Include(s => s.User)
                .Where(s => s.CurrentBelt == beltRank)
                .ToListAsync(cancellationToken);
        }

        public async Task<Student?> GetStudentWithAttendancesAsync(Guid studentId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(s => !s.IsDeleted)
                .Include(s => s.User)
                .Include(s => s.Attendances)
                    .ThenInclude(a => a.TrainingSession)
                .FirstOrDefaultAsync(s => s.Id == studentId, cancellationToken);
        }

        public async Task<Student?> GetStudentWithBeltPromotionsAsync(Guid studentId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(s => !s.IsDeleted)
                .Include(s => s.User)
                .Include(s => s.BeltPromotions)
                    .ThenInclude(bp => bp.PromotedBy)
                        .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(s => s.Id == studentId, cancellationToken);
        }

        public async Task<IEnumerable<Student>> GetStudentsEligibleForGradingAsync(CancellationToken cancellationToken = default)
        {
            var currentDate = DateTime.UtcNow;
            return await _dbSet
                .Where(s => !s.IsDeleted)
                .Include(s => s.User)
                .Where(s => s.Status == StudentStatus.Active && 
                           (s.NextGradingEligible == null || s.NextGradingEligible <= currentDate))
                .ToListAsync(cancellationToken);
        }
    }
}
