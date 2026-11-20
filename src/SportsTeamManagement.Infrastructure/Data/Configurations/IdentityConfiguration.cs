using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportsTeamManagement.Domain.Entities;

namespace SportsTeamManagement.Infrastructure.Data.Configurations;

public sealed class ExternalLoginConfiguration : IEntityTypeConfiguration<ExternalLogin>
{
    public void Configure(EntityTypeBuilder<ExternalLogin> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Provider).IsRequired().HasMaxLength(100);
        builder.Property(x => x.ProviderUserId).IsRequired().HasMaxLength(255);
        builder.Property(x => x.Email).HasMaxLength(255);
        builder.HasIndex(x => new { x.Provider, x.ProviderUserId }).IsUnique();
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TokenHash).IsRequired().HasMaxLength(64);
        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.HasIndex(x => x.TokenFamilyId);
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class EnrollmentRequestConfiguration : IEntityTypeConfiguration<EnrollmentRequest>
{
    public void Configure(EntityTypeBuilder<EnrollmentRequest> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RequestedRole).HasConversion<int>();
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.Message).HasMaxLength(2000);
        builder.Property(x => x.ReviewNotes).HasMaxLength(2000);
        builder.HasIndex(x => new { x.UserId, x.ClubId, x.RequestedRole, x.Status });
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Club).WithMany().HasForeignKey(x => x.ClubId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class ParentStudentRelationshipConfiguration : IEntityTypeConfiguration<ParentStudentRelationship>
{
    public void Configure(EntityTypeBuilder<ParentStudentRelationship> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.VerificationStatus).HasConversion<int>();
        builder.Property(x => x.RelationshipType).HasMaxLength(100);
        builder.HasIndex(x => new { x.ParentUserId, x.StudentUserId, x.ClubId }).IsUnique();
        builder.HasOne(x => x.ParentUser).WithMany().HasForeignKey(x => x.ParentUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.StudentUser).WithMany().HasForeignKey(x => x.StudentUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Club).WithMany().HasForeignKey(x => x.ClubId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class RoleAssignmentHistoryConfiguration : IEntityTypeConfiguration<RoleAssignmentHistory>
{
    public void Configure(EntityTypeBuilder<RoleAssignmentHistory> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PreviousRole).HasConversion<int>();
        builder.Property(x => x.NewRole).HasConversion<int>();
        builder.Property(x => x.Reason).HasMaxLength(2000);
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.AssignedByUser).WithMany().HasForeignKey(x => x.AssignedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Club).WithMany().HasForeignKey(x => x.ClubId).OnDelete(DeleteBehavior.SetNull);
    }
}

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Action).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Details).HasMaxLength(4000);
        builder.Property(x => x.CorrelationId).HasMaxLength(100);
        builder.HasIndex(x => new { x.ClubId, x.CreatedAt });
        builder.HasIndex(x => new { x.ActorUserId, x.CreatedAt });
        builder.HasOne(x => x.ActorUser).WithMany().HasForeignKey(x => x.ActorUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.TargetUser).WithMany().HasForeignKey(x => x.TargetUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Club).WithMany().HasForeignKey(x => x.ClubId).OnDelete(DeleteBehavior.SetNull);
    }
}
