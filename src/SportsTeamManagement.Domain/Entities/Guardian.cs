using SportsTeamManagement.Domain.Common;

namespace SportsTeamManagement.Domain.Entities;

public sealed class Guardian : BaseEntity
{
    public Guid ClubId { get; set; }
    public Guid UserId { get; set; }
    public string Relationship { get; set; } = string.Empty;
    public bool IsEmergencyContact { get; set; }
    public bool CanViewMedicalInformation { get; set; }

    public Club Club { get; set; } = null!;
    public User User { get; set; } = null!;
    public ICollection<GuardianStudent> Students { get; set; } = new List<GuardianStudent>();
}

public sealed class GuardianStudent : BaseEntity
{
    public Guid GuardianId { get; set; }
    public Guid StudentId { get; set; }
    public bool IsAuthorized { get; set; } = true;
    public Guardian Guardian { get; set; } = null!;
    public Student Student { get; set; } = null!;
}
