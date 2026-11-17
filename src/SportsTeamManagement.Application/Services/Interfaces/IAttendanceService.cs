using SportsTeamManagement.Application.Common.Models;
using SportsTeamManagement.Application.DTOs;

namespace SportsTeamManagement.Application.Services.Interfaces
{
    public interface IAttendanceService
    {
        Task<Result<AttendanceDto>> MarkAttendanceAsync(CreateAttendanceDto createAttendanceDto, CancellationToken cancellationToken = default);
        Task<Result<AttendanceDto>> UpdateAttendanceAsync(Guid attendanceId, UpdateAttendanceDto updateAttendanceDto, CancellationToken cancellationToken = default);
        Task<Result<IEnumerable<AttendanceDto>>> GetAttendancesByStudentAsync(Guid studentId, CancellationToken cancellationToken = default);
        Task<Result<IEnumerable<AttendanceDto>>> GetAttendancesByTrainingSessionAsync(Guid trainingSessionId, CancellationToken cancellationToken = default);
        Task<Result<IEnumerable<AttendanceDto>>> GetAttendancesByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<Result<AttendanceStatsDto>> GetAttendanceStatsAsync(Guid studentId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
        Task<Result<IEnumerable<AttendanceStatsDto>>> GetAllStudentsAttendanceStatsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    }
}
