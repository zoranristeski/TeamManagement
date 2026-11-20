namespace SportsTeamManagement.Domain.Enums;

public enum EnrollmentRequestStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Cancelled = 4,
    Withdrawn = 5
}

public enum RelationshipVerificationStatus
{
    Pending = 1,
    Verified = 2,
    Rejected = 3,
    Revoked = 4
}
