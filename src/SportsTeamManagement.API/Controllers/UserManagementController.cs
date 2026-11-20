using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsTeamManagement.Application.DTOs;
using SportsTeamManagement.Domain.Entities;
using SportsTeamManagement.Domain.Enums;
using SportsTeamManagement.Infrastructure.Data;

namespace SportsTeamManagement.API.Controllers;

[ApiController]
[Route("api/user-management")]
[Authorize]
public sealed class UserManagementController(ApplicationDbContext db) : BaseApiController
{
    [HttpPost("enrollment-requests")]
    public async Task<IActionResult> CreateEnrollment([FromBody] CreateEnrollmentRequestDto request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();
        if (request.RequestedRole is not (UserRole.Student or UserRole.Parent))
            return BadRequest("Only Student and Parent enrollment requests are allowed.");
        if (!await db.Clubs.AnyAsync(x => x.Id == request.ClubId && x.IsActive, cancellationToken))
            return NotFound("Club not found.");
        if (await db.EnrollmentRequests.AnyAsync(x =>
            x.UserId == userId &&
            x.ClubId == request.ClubId &&
            x.RequestedRole == request.RequestedRole &&
            x.Status == EnrollmentRequestStatus.Pending, cancellationToken))
            return Conflict("A pending enrollment request already exists.");

        var enrollment = new EnrollmentRequest
        {
            UserId = userId.Value,
            ClubId = request.ClubId,
            RequestedRole = request.RequestedRole,
            Status = EnrollmentRequestStatus.Pending,
            Message = request.Message
        };
        db.EnrollmentRequests.Add(enrollment);
        await db.SaveChangesAsync(cancellationToken);
        await AddAuditAsync(userId, enrollment.UserId, request.ClubId, "EnrollmentRequested",
            $"Requested {request.RequestedRole} membership.", cancellationToken);
        return Ok(enrollment);
    }

    [HttpGet("enrollment-requests/my")]
    public async Task<IActionResult> GetMyEnrollmentRequests(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();
        return Ok(await db.EnrollmentRequests
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken));
    }

    [HttpGet("clubs/{clubId:guid}/enrollment-requests")]
    [Authorize(Policy = "ClubTrainer")]
    public async Task<IActionResult> GetClubEnrollmentRequests(Guid clubId, CancellationToken cancellationToken)
        => Ok(await db.EnrollmentRequests
            .AsNoTracking()
            .Where(x => x.ClubId == clubId && x.Status == EnrollmentRequestStatus.Pending)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken));

    [HttpPost("enrollment-requests/{requestId:guid}/approve")]
    [Authorize(Policy = "ClubTrainer")]
    public async Task<IActionResult> ApproveEnrollment(Guid requestId, [FromBody] ReviewEnrollmentRequestDto review, CancellationToken cancellationToken)
    {
        var actorId = GetUserId();
        if (actorId is null)
            return Unauthorized();
        var request = await db.EnrollmentRequests.SingleOrDefaultAsync(x => x.Id == requestId, cancellationToken);
        if (request is null)
            return NotFound();
        if (!await IsClubAuthorizedAsync(actorId.Value, request.ClubId, cancellationToken))
            return Forbid();
        if (request.Status != EnrollmentRequestStatus.Pending)
            return Conflict("The enrollment request is no longer pending.");

        var membership = await db.ClubMemberships.SingleOrDefaultAsync(x =>
            x.UserId == request.UserId && x.ClubId == request.ClubId, cancellationToken);
        if (membership is null)
        {
            membership = new ClubMembership
            {
                UserId = request.UserId,
                ClubId = request.ClubId,
                ClubRole = request.RequestedRole,
                Status = MembershipStatus.Active,
                IsActive = true,
                ApprovedAt = DateTime.UtcNow,
                ApprovedByUserId = actorId
            };
            db.ClubMemberships.Add(membership);
        }
        else
        {
            membership.ClubRole = request.RequestedRole;
            membership.Status = MembershipStatus.Active;
            membership.IsActive = true;
            membership.ApprovedAt = DateTime.UtcNow;
            membership.ApprovedByUserId = actorId;
        }

        request.Status = EnrollmentRequestStatus.Approved;
        request.ReviewedByUserId = actorId;
        request.ReviewedAt = DateTime.UtcNow;
        request.ReviewNotes = review.ReviewNotes;
        db.RoleAssignmentHistories.Add(new RoleAssignmentHistory
        {
            UserId = request.UserId,
            ClubId = request.ClubId,
            PreviousRole = null,
            NewRole = request.RequestedRole,
            AssignedByUserId = actorId.Value,
            Reason = "Enrollment request approved"
        });
        await db.SaveChangesAsync(cancellationToken);
        await AddAuditAsync(actorId, request.UserId, request.ClubId, "EnrollmentApproved",
            $"Approved {request.RequestedRole} membership.", cancellationToken);
        return Ok(membership);
    }

    [HttpPost("enrollment-requests/{requestId:guid}/reject")]
    [Authorize(Policy = "ClubTrainer")]
    public async Task<IActionResult> RejectEnrollment(Guid requestId, [FromBody] ReviewEnrollmentRequestDto review, CancellationToken cancellationToken)
    {
        var actorId = GetUserId();
        if (actorId is null)
            return Unauthorized();
        var request = await db.EnrollmentRequests.SingleOrDefaultAsync(x => x.Id == requestId, cancellationToken);
        if (request is null)
            return NotFound();
        if (!await IsClubAuthorizedAsync(actorId.Value, request.ClubId, cancellationToken))
            return Forbid();
        if (request.Status != EnrollmentRequestStatus.Pending)
            return Conflict("The enrollment request is no longer pending.");

        request.Status = EnrollmentRequestStatus.Rejected;
        request.ReviewedByUserId = actorId;
        request.ReviewedAt = DateTime.UtcNow;
        request.ReviewNotes = review.ReviewNotes;
        await db.SaveChangesAsync(cancellationToken);
        await AddAuditAsync(actorId, request.UserId, request.ClubId, "EnrollmentRejected",
            "Rejected enrollment request.", cancellationToken);
        return Ok(request);
    }

    [HttpPost("clubs/members/role")]
    [Authorize(Policy = "ClubTrainer")]
    public async Task<IActionResult> AssignRole([FromBody] AssignClubRoleDto request, CancellationToken cancellationToken)
    {
        var actorId = GetUserId();
        if (actorId is null)
            return Unauthorized();
        if (!await IsClubAuthorizedAsync(actorId.Value, request.ClubId, cancellationToken))
            return Forbid();
        if (request.Role is not (UserRole.Student or UserRole.Parent or UserRole.Trainer))
            return BadRequest("Club role assignment is restricted to Student, Parent, and Trainer.");

        var membership = await db.ClubMemberships.SingleOrDefaultAsync(x =>
            x.UserId == request.UserId && x.ClubId == request.ClubId, cancellationToken);
        var previousRole = membership?.ClubRole;
        if (membership is null)
        {
            membership = new ClubMembership
            {
                UserId = request.UserId,
                ClubId = request.ClubId,
                ClubRole = request.Role,
                Status = MembershipStatus.Active,
                IsActive = true,
                ApprovedAt = DateTime.UtcNow,
                ApprovedByUserId = actorId
            };
            db.ClubMemberships.Add(membership);
        }
        else
        {
            membership.ClubRole = request.Role;
            membership.Status = MembershipStatus.Active;
            membership.IsActive = true;
            membership.ApprovedAt = DateTime.UtcNow;
            membership.ApprovedByUserId = actorId;
        }
        db.RoleAssignmentHistories.Add(new RoleAssignmentHistory
        {
            UserId = request.UserId,
            ClubId = request.ClubId,
            PreviousRole = previousRole,
            NewRole = request.Role,
            AssignedByUserId = actorId.Value,
            Reason = request.Reason
        });
        await db.SaveChangesAsync(cancellationToken);
        await AddAuditAsync(actorId, request.UserId, request.ClubId, "ClubRoleAssigned",
            $"Assigned {request.Role}.", cancellationToken);
        return Ok(membership);
    }

    [HttpPost("parent-relations")]
    [Authorize(Policy = "ClubTrainer")]
    public async Task<IActionResult> CreateParentRelation([FromBody] ParentRelationshipDto request, CancellationToken cancellationToken)
    {
        var actorId = GetUserId();
        if (actorId is null)
            return Unauthorized();
        if (!await IsClubAuthorizedAsync(actorId.Value, request.ClubId, cancellationToken))
            return Forbid();
        if (await db.ParentStudentRelationships.AnyAsync(x =>
            x.ParentUserId == request.ParentUserId &&
            x.StudentUserId == request.StudentUserId &&
            x.ClubId == request.ClubId, cancellationToken))
            return Conflict("The relationship already exists.");

        var relationship = new ParentStudentRelationship
        {
            ParentUserId = request.ParentUserId,
            StudentUserId = request.StudentUserId,
            ClubId = request.ClubId,
            RelationshipType = request.RelationshipType,
            VerificationStatus = RelationshipVerificationStatus.Pending
        };
        db.ParentStudentRelationships.Add(relationship);
        await db.SaveChangesAsync(cancellationToken);
        await AddAuditAsync(actorId, request.StudentUserId, request.ClubId, "ParentRelationshipCreated",
            "Parent-student relationship submitted for verification.", cancellationToken);
        return Ok(relationship);
    }

    private async Task<bool> IsClubAuthorizedAsync(Guid userId, Guid clubId, CancellationToken cancellationToken)
    {
        if (User.IsInRole("PlatformAdmin"))
            return true;
        return await db.ClubMemberships.AsNoTracking().AnyAsync(x =>
            x.ClubId == clubId &&
            x.UserId == userId &&
            x.IsActive &&
            x.Status == MembershipStatus.Active &&
            (x.ClubRole == UserRole.Trainer || x.ClubRole == UserRole.ClubAdmin || x.ClubRole == UserRole.Admin),
            cancellationToken);
    }

    private Guid? GetUserId()
    {
        var subject = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(subject, out var userId) ? userId : null;
    }

    private async Task AddAuditAsync(Guid? actorId, Guid? targetId, Guid? clubId, string action, string details, CancellationToken cancellationToken)
    {
        db.AuditLogs.Add(new AuditLog
        {
            ActorUserId = actorId,
            TargetUserId = targetId,
            ClubId = clubId,
            Action = action,
            Details = details,
            CorrelationId = HttpContext.TraceIdentifier
        });
        await db.SaveChangesAsync(cancellationToken);
    }
}
