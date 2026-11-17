using SportsTeamManagement.Domain.Enums;

namespace SportsTeamManagement.Application.DTOs
{
    public class EventDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public EventType Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Location { get; set; }
        public int MaxParticipants { get; set; }
        public decimal? Cost { get; set; }
        public bool IsActive { get; set; }
        public DateTime RegistrationDeadline { get; set; }
        public Guid OrganizerId { get; set; }
        public TrainerDto Organizer { get; set; } = null!;
        public TimeSpan Duration { get; set; }
        public int RegisteredCount { get; set; }
        public bool IsRegistrationOpen { get; set; }
    }

    public class CreateEventDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public EventType Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Location { get; set; }
        public int MaxParticipants { get; set; } = 100;
        public decimal? Cost { get; set; }
        public DateTime RegistrationDeadline { get; set; }
        public Guid OrganizerId { get; set; }
    }

    public class UpdateEventDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public EventType Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Location { get; set; }
        public int MaxParticipants { get; set; }
        public decimal? Cost { get; set; }
        public bool IsActive { get; set; }
        public DateTime RegistrationDeadline { get; set; }
    }

    public class EventRegistrationDto
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public Guid EventId { get; set; }
        public StudentDto Student { get; set; } = null!;
        public EventDto Event { get; set; } = null!;
        public DateTime RegistrationDate { get; set; }
        public bool IsConfirmed { get; set; }
        public bool IsAttended { get; set; }
        public string? Notes { get; set; }
        public decimal? AmountPaid { get; set; }
    }
}
