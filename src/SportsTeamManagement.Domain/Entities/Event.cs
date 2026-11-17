using SportsTeamManagement.Domain.Common;
using SportsTeamManagement.Domain.Enums;

namespace SportsTeamManagement.Domain.Entities
{
    public class Event : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public EventType Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Location { get; set; }
        public int MaxParticipants { get; set; } = 100;
        public decimal? Cost { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime RegistrationDeadline { get; set; }
        public Guid OrganizerId { get; set; }

        // Navigation properties
        public Trainer Organizer { get; set; } = null!;
        public ICollection<EventRegistration> Registrations { get; set; } = new List<EventRegistration>();

        public TimeSpan Duration => EndDate - StartDate;
        public int RegisteredCount => Registrations.Count(r => r.IsConfirmed);
        public bool IsRegistrationOpen => DateTime.UtcNow <= RegistrationDeadline && IsActive;
    }
}
