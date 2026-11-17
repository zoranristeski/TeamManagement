using SportsTeamManagement.Domain.Common;
using SportsTeamManagement.Domain.Enums;

namespace SportsTeamManagement.Domain.Entities
{
    public class Trainer : BaseEntity
    {
        public Guid UserId { get; set; }
        public BeltRank Rank { get; set; }
        public string? Specialization { get; set; }
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public string? Biography { get; set; }
        public bool CanApproveStudents { get; set; } = false;
        public bool CanPromoteBelts { get; set; } = false;

        // Navigation properties
        public User User { get; set; } = null!;
        public ICollection<TrainingSession> TrainingSessions { get; set; } = new List<TrainingSession>();
        public ICollection<Event> Events { get; set; } = new List<Event>();
        public ICollection<BeltPromotion> BeltPromotions { get; set; } = new List<BeltPromotion>();
    }
}
