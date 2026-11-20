using SportsTeamManagement.Domain.Enums;

namespace SportsTeamManagement.Application.DTOs;

public sealed class CreateEnrollmentRequestDto
{
    public Guid ClubId { get; set; }
    public UserRole RequestedRole { get; set; } = UserRole.Student;
    public string? Message { get; set; }
}

public sealed class ReviewEnrollmentRequestDto
{
    public string? ReviewNotes { get; set; }
}

public sealed class AssignClubRoleDto
{
    public Guid UserId { get; set; }
    public Guid ClubId { get; set; }
    public UserRole Role { get; set; }
    public string? Reason { get; set; }
}

public sealed class ParentRelationshipDto
{
    public Guid ParentUserId { get; set; }
    public Guid StudentUserId { get; set; }
    public Guid ClubId { get; set; }
    public string? RelationshipType { get; set; }
}
