using SportsTeamManagement.Domain.Enums;

namespace SportsTeamManagement.Application.DTOs
{
    public class TrainerDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public UserDto User { get; set; } = null!;
        public BeltRank Rank { get; set; }
        public string? Specialization { get; set; }
        public DateTime StartDate { get; set; }
        public string? Biography { get; set; }
        public bool CanApproveStudents { get; set; }
        public bool CanPromoteBelts { get; set; }
    }

    public class CreateTrainerDto
    {
        public Guid UserId { get; set; }
        public BeltRank Rank { get; set; }
        public string? Specialization { get; set; }
        public string? Biography { get; set; }
        public bool CanApproveStudents { get; set; }
        public bool CanPromoteBelts { get; set; }
    }

    public class UpdateTrainerDto
    {
        public BeltRank Rank { get; set; }
        public string? Specialization { get; set; }
        public string? Biography { get; set; }
        public bool CanApproveStudents { get; set; }
        public bool CanPromoteBelts { get; set; }
    }
}
