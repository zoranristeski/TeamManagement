using Microsoft.EntityFrameworkCore;
using SportsTeamManagement.Application.Common.Interfaces;
using SportsTeamManagement.Domain.Entities;
using SportsTeamManagement.Infrastructure.Data;

namespace SportsTeamManagement.Infrastructure.Repositories
{
    public class AttendanceRepository : Repository<Attendance>, IAttendanceRepository
    {
        public AttendanceRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Attendance>> GetAttendancesByStudentAsync(Guid studentId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(a => !a.IsDeleted)
                .Include(a => a.TrainingSession)
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Where(a => a.StudentId == studentId)
                .OrderByDescending(a => a.AttendanceDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Attendance>> GetAttendancesByTrainingSessionAsync(Guid trainingSessionId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(a => !a.IsDeleted)
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.TrainingSession)
                .Where(a => a.TrainingSessionId == trainingSessionId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Attendance>> GetAttendancesByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(a => !a.IsDeleted)
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.TrainingSession)
                .Where(a => a.AttendanceDate >= startDate && a.AttendanceDate <= endDate)
                .OrderByDescending(a => a.AttendanceDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetAttendanceCountByStudentAsync(Guid studentId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .Where(a => !a.IsDeleted)
                .Where(a => a.StudentId == studentId && a.IsPresent);

            if (startDate.HasValue)
                query = query.Where(a => a.AttendanceDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.AttendanceDate <= endDate.Value);

            return await query.CountAsync(cancellationToken);
        }

        public async Task<Attendance?> GetAttendanceByStudentAndSessionAsync(Guid studentId, Guid trainingSessionId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(a => !a.IsDeleted)
                .FirstOrDefaultAsync(a => a.StudentId == studentId && a.TrainingSessionId == trainingSessionId, cancellationToken);
        }
    }
}
