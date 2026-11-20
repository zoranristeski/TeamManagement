using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SportsTeamManagement.Domain.Enums;
using SportsTeamManagement.Infrastructure.Data;

namespace SportsTeamManagement.API.Authorization;

public sealed class ClubRoleRequirement(params UserRole[] allowedRoles) : IAuthorizationRequirement
{
    public IReadOnlyCollection<UserRole> AllowedRoles { get; } = allowedRoles;
}

public sealed class ClubRoleHandler(ApplicationDbContext db) : AuthorizationHandler<ClubRoleRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ClubRoleRequirement requirement)
    {
        if (context.User.IsInRole("PlatformAdmin"))
        {
            context.Succeed(requirement);
            return;
        }

        var hasAllowedRole = context.User.Claims
            .Where(x => x.Type is "roles" or ClaimTypes.Role)
            .Select(x => x.Value)
            .Any(role => requirement.AllowedRoles.Any(allowed =>
                string.Equals(role, allowed.ToString(), StringComparison.OrdinalIgnoreCase)));
        if (!hasAllowedRole)
            return;

        var clubId = GetClubId(context.Resource);
        if (clubId is null)
        {
            context.Succeed(requirement);
            return;
        }

        var subject = context.User.FindFirstValue("sub");
        if (!Guid.TryParse(subject, out var userId))
            return;

        var membership = await db.ClubMemberships
            .AsNoTracking()
            .SingleOrDefaultAsync(x =>
                x.ClubId == clubId &&
                x.UserId == userId &&
                x.IsActive &&
                x.Status == MembershipStatus.Active);
        if (membership is not null &&
            requirement.AllowedRoles.Contains(membership.ClubRole))
            context.Succeed(requirement);
    }

    private static Guid? GetClubId(object? resource)
    {
        if (resource is not HttpContext httpContext)
            return null;
        if (httpContext.Request.Headers.TryGetValue("X-Club-Id", out var header) &&
            Guid.TryParse(header.ToString(), out var headerClubId))
            return headerClubId;
        if (httpContext.Request.Query.TryGetValue("clubId", out var query) &&
            Guid.TryParse(query.ToString(), out var queryClubId))
            return queryClubId;
        if (httpContext.Request.RouteValues.TryGetValue("clubId", out var route) &&
            Guid.TryParse(route?.ToString(), out var routeClubId))
            return routeClubId;
        return null;
    }
}
