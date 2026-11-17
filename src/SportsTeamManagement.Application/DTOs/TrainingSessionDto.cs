namespace SportsTeamManagement.Application.DTOs
{
    public class TrainingSessionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public Guid TrainerId { get; set; }
        public TrainerDto Trainer { get; set; } = null!;
        public int MaxParticipants { get; set; }
        public bool IsActive { get; set; }
        public TimeSpan Duration { get; set; }
        public int AttendeeCount { get; set; }
    }

    public class CreateTrainingSessionDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public Guid TrainerId { get; set; }
        public int MaxParticipants { get; set; } = 50;
    }

    public class UpdateTrainingSessionDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int MaxParticipants { get; set; }
        public bool IsActive { get; set; }
    }
}
