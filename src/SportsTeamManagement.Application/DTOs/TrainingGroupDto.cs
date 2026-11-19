namespace SportsTeamManagement.Application.DTOs;

public sealed class TrainingGroupDto
{
    public Guid Id { get; set; }
    public Guid ClubId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SportOrDiscipline { get; set; }
    public int? MinimumAge { get; set; }
    public int? MaximumAge { get; set; }
    public string? SkillLevel { get; set; }
    public int Capacity { get; set; }
    public string? Location { get; set; }
    public bool IsActive { get; set; }
}

public sealed class CreateTrainingGroupDto
{
    public Guid ClubId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SportOrDiscipline { get; set; }
    public int? MinimumAge { get; set; }
    public int? MaximumAge { get; set; }
    public string? SkillLevel { get; set; }
    public int Capacity { get; set; } = 20;
    public string? Location { get; set; }
}
