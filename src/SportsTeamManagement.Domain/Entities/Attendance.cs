using SportsTeamManagement.Domain.Common;

namespace SportsTeamManagement.Domain.Entities
{
    public class Attendance : BaseEntity
    {
        public Guid StudentId { get; set; }
        public Guid TrainingSessionId { get; set; }
        public DateTime AttendanceDate { get; set; }
        public bool IsPresent { get; set; } = true;
        public string? Notes { get; set; }

        // Navigation properties
        public Student Student { get; set; } = null!;
        public TrainingSession TrainingSession { get; set; } = null!;
    }
}
