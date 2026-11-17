using SportsTeamManagement.Domain.Entities;
using SportsTeamManagement.Domain.Enums;

namespace SportsTeamManagement.Application.Common.Interfaces
{
    public interface IStudentRepository : IRepository<Student>
    {
        Task<IEnumerable<Student>> GetStudentsByStatusAsync(StudentStatus status, CancellationToken cancellationToken = default);
        Task<IEnumerable<Student>> GetStudentsByBeltRankAsync(BeltRank beltRank, CancellationToken cancellationToken = default);
        Task<Student?> GetStudentWithAttendancesAsync(Guid studentId, CancellationToken cancellationToken = default);
        Task<Student?> GetStudentWithBeltPromotionsAsync(Guid studentId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Student>> GetStudentsEligibleForGradingAsync(CancellationToken cancellationToken = default);
    }
}
