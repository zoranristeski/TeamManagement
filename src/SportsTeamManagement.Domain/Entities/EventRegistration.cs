using SportsTeamManagement.Domain.Common;

namespace SportsTeamManagement.Domain.Entities
{
    public class EventRegistration : BaseEntity
    {
        public Guid StudentId { get; set; }
        public Guid EventId { get; set; }
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        public bool IsConfirmed { get; set; } = false;
        public bool IsAttended { get; set; } = false;
        public string? Notes { get; set; }
        public decimal? AmountPaid { get; set; }

        // Navigation properties
        public Student Student { get; set; } = null!;
        public Event Event { get; set; } = null!;
    }
}
