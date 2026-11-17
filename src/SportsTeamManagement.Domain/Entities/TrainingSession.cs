using SportsTeamManagement.Domain.Common;

namespace SportsTeamManagement.Domain.Entities
{
    public class TrainingSession : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public Guid TrainerId { get; set; }
        public int MaxParticipants { get; set; } = 50;
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public Trainer Trainer { get; set; } = null!;
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

        public TimeSpan Duration => EndTime - StartTime;
        public int AttendeeCount => Attendances.Count;
    }
}
