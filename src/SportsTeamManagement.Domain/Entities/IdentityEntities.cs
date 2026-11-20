using SportsTeamManagement.Domain.Common;
using SportsTeamManagement.Domain.Enums;

namespace SportsTeamManagement.Domain.Entities;

public sealed class ExternalLogin : BaseEntity
{
    public Guid UserId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string ProviderUserId { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool EmailVerified { get; set; }
    public User User { get; set; } = null!;
}

public sealed class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public Guid TokenFamilyId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public Guid? ReplacedByTokenId { get; set; }
    public string? RevocationReason { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public User User { get; set; } = null!;
}

public sealed class EnrollmentRequest : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid ClubId { get; set; }
    public UserRole RequestedRole { get; set; }
    public EnrollmentRequestStatus Status { get; set; } = EnrollmentRequestStatus.Pending;
    public string? Message { get; set; }
    public Guid? ReviewedByUserId { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNotes { get; set; }
    public User User { get; set; } = null!;
    public Club Club { get; set; } = null!;
}

public sealed class ParentStudentRelationship : BaseEntity
{
    public Guid ParentUserId { get; set; }
    public Guid StudentUserId { get; set; }
    public Guid ClubId { get; set; }
    public RelationshipVerificationStatus VerificationStatus { get; set; } = RelationshipVerificationStatus.Pending;
    public string? RelationshipType { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public Guid? VerifiedByUserId { get; set; }
    public bool IsActive { get; set; } = true;
    public User ParentUser { get; set; } = null!;
    public User StudentUser { get; set; } = null!;
    public Club Club { get; set; } = null!;
}

public sealed class RoleAssignmentHistory : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid? ClubId { get; set; }
    public UserRole? PreviousRole { get; set; }
    public UserRole NewRole { get; set; }
    public Guid AssignedByUserId { get; set; }
    public string? Reason { get; set; }
    public User User { get; set; } = null!;
    public User AssignedByUser { get; set; } = null!;
    public Club? Club { get; set; }
}

public sealed class AuditLog : BaseEntity
{
    public Guid? ActorUserId { get; set; }
    public Guid? TargetUserId { get; set; }
    public Guid? ClubId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? CorrelationId { get; set; }
    public User? ActorUser { get; set; }
    public User? TargetUser { get; set; }
    public Club? Club { get; set; }
}
