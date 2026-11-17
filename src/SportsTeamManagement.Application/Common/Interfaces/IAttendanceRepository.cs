using SportsTeamManagement.Domain.Entities;

namespace SportsTeamManagement.Application.Common.Interfaces
{
    public interface IAttendanceRepository : IRepository<Attendance>
    {
        Task<IEnumerable<Attendance>> GetAttendancesByStudentAsync(Guid studentId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Attendance>> GetAttendancesByTrainingSessionAsync(Guid trainingSessionId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Attendance>> GetAttendancesByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<int> GetAttendanceCountByStudentAsync(Guid studentId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
        Task<Attendance?> GetAttendanceByStudentAndSessionAsync(Guid studentId, Guid trainingSessionId, CancellationToken cancellationToken = default);
    }
}
