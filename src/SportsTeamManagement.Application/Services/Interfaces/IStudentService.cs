using SportsTeamManagement.Application.Common.Models;
using SportsTeamManagement.Application.DTOs;
using SportsTeamManagement.Domain.Enums;

namespace SportsTeamManagement.Application.Services.Interfaces
{
    public interface IStudentService
    {
        Task<Result<StudentDto>> GetStudentByIdAsync(Guid studentId, CancellationToken cancellationToken = default);
        Task<Result<StudentDto>> GetStudentByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<Result<StudentDto>> RegisterStudentAsync(StudentRegistrationDto registrationDto, CancellationToken cancellationToken = default);
        Task<Result<StudentDto>> UpdateStudentAsync(Guid studentId, UpdateStudentDto updateStudentDto, CancellationToken cancellationToken = default);
        Task<Result> ApproveStudentAsync(Guid studentId, Guid approvedById, CancellationToken cancellationToken = default);
        Task<Result> RejectStudentAsync(Guid studentId, string reason, CancellationToken cancellationToken = default);
        Task<Result<IEnumerable<StudentDto>>> GetStudentsByStatusAsync(StudentStatus status, CancellationToken cancellationToken = default);
        Task<Result<IEnumerable<StudentDto>>> GetStudentsByBeltRankAsync(BeltRank beltRank, CancellationToken cancellationToken = default);
        Task<Result<IEnumerable<StudentDto>>> GetStudentsEligibleForGradingAsync(CancellationToken cancellationToken = default);
        Task<Result<AttendanceStatsDto>> GetStudentAttendanceStatsAsync(Guid studentId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    }
}
