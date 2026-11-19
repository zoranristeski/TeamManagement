using SportsTeamManagement.Domain.Common;
using SportsTeamManagement.Domain.Enums;

namespace SportsTeamManagement.Domain.Entities;

public sealed class MembershipPlan : BaseEntity
{
    public Guid ClubId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal MonthlyPrice { get; set; }
    public bool IsActive { get; set; } = true;
    public Club Club { get; set; } = null!;
}

public sealed class Membership : BaseEntity
{
    public Guid ClubId { get; set; }
    public Guid StudentId { get; set; }
    public Guid MembershipPlanId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public MembershipStatus Status { get; set; }

    public Club Club { get; set; } = null!;
    public Student Student { get; set; } = null!;
    public MembershipPlan Plan { get; set; } = null!;
    public ICollection<PaymentRecord> Payments { get; set; } = new List<PaymentRecord>();
}

public sealed class PaymentRecord : BaseEntity
{
    public Guid ClubId { get; set; }
    public Guid MembershipId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public PaymentStatus Status { get; set; }
    public string? Reference { get; set; }
    public Membership Membership { get; set; } = null!;
}
