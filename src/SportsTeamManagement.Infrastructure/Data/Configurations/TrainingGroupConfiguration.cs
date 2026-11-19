using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportsTeamManagement.Domain.Entities;

namespace SportsTeamManagement.Infrastructure.Data.Configurations;

public sealed class TrainingGroupConfiguration : IEntityTypeConfiguration<TrainingGroup>
{
    public void Configure(EntityTypeBuilder<TrainingGroup> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.SportOrDiscipline).HasMaxLength(100);
        builder.Property(x => x.SkillLevel).HasMaxLength(100);
        builder.Property(x => x.Location).HasMaxLength(300);
        builder.HasIndex(x => new { x.ClubId, x.Name });
        builder.HasOne(x => x.Club).WithMany().HasForeignKey(x => x.ClubId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class TrainingGroupStudentConfiguration : IEntityTypeConfiguration<TrainingGroupStudent>
{
    public void Configure(EntityTypeBuilder<TrainingGroupStudent> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.TrainingGroupId, x.StudentId }).IsUnique();
        builder.HasOne(x => x.TrainingGroup).WithMany(x => x.Students).HasForeignKey(x => x.TrainingGroupId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class TrainingGroupTrainerConfiguration : IEntityTypeConfiguration<TrainingGroupTrainer>
{
    public void Configure(EntityTypeBuilder<TrainingGroupTrainer> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.TrainingGroupId, x.TrainerId }).IsUnique();
        builder.HasOne(x => x.TrainingGroup).WithMany(x => x.Trainers).HasForeignKey(x => x.TrainingGroupId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Trainer).WithMany().HasForeignKey(x => x.TrainerId).OnDelete(DeleteBehavior.Restrict);
    }
}
