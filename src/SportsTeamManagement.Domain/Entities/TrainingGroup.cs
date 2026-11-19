using SportsTeamManagement.Domain.Common;

namespace SportsTeamManagement.Domain.Entities;

public sealed class TrainingGroup : BaseEntity
{
    public Guid ClubId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SportOrDiscipline { get; set; }
    public int? MinimumAge { get; set; }
    public int? MaximumAge { get; set; }
    public string? SkillLevel { get; set; }
    public int Capacity { get; set; }
    public string? Location { get; set; }
    public bool IsActive { get; set; } = true;

    public Club Club { get; set; } = null!;
    public ICollection<TrainingGroupStudent> Students { get; set; } = new List<TrainingGroupStudent>();
    public ICollection<TrainingGroupTrainer> Trainers { get; set; } = new List<TrainingGroupTrainer>();
}

public sealed class TrainingGroupStudent : BaseEntity
{
    public Guid TrainingGroupId { get; set; }
    public Guid StudentId { get; set; }
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public TrainingGroup TrainingGroup { get; set; } = null!;
    public Student Student { get; set; } = null!;
}

public sealed class TrainingGroupTrainer : BaseEntity
{
    public Guid TrainingGroupId { get; set; }
    public Guid TrainerId { get; set; }
    public bool IsActive { get; set; } = true;
    public TrainingGroup TrainingGroup { get; set; } = null!;
    public Trainer Trainer { get; set; } = null!;
}
