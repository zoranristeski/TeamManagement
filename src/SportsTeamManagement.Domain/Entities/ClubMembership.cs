using SportsTeamManagement.Domain.Common;
using SportsTeamManagement.Domain.Enums;

namespace SportsTeamManagement.Domain.Entities;

public sealed class ClubMembership : BaseEntity
{
    public Guid ClubId { get; set; }
    public Guid UserId { get; set; }
    public UserRole ClubRole { get; set; }
    public bool IsActive { get; set; } = true;
    public Club Club { get; set; } = null!;
    public User User { get; set; } = null!;
}
