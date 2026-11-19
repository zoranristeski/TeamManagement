using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportsTeamManagement.Domain.Entities;

namespace SportsTeamManagement.Infrastructure.Data.Configurations;

public sealed class GuardianConfiguration : IEntityTypeConfiguration<Guardian>
{
    public void Configure(EntityTypeBuilder<Guardian> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Relationship).IsRequired().HasMaxLength(100);
        builder.HasIndex(x => new { x.ClubId, x.UserId }).IsUnique();
        builder.HasOne(x => x.Club).WithMany().HasForeignKey(x => x.ClubId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class GuardianStudentConfiguration : IEntityTypeConfiguration<GuardianStudent>
{
    public void Configure(EntityTypeBuilder<GuardianStudent> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.GuardianId, x.StudentId }).IsUnique();
        builder.HasOne(x => x.Guardian).WithMany(x => x.Students).HasForeignKey(x => x.GuardianId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
    }
}
