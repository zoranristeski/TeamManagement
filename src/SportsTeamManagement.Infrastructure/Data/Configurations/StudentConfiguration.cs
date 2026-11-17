using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportsTeamManagement.Domain.Entities;

namespace SportsTeamManagement.Infrastructure.Data.Configurations
{
    public class StudentConfiguration : IEntityTypeConfiguration<Student>
    {
        public void Configure(EntityTypeBuilder<Student> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.UserId)
                .IsRequired();

            builder.Property(s => s.DateOfBirth)
                .IsRequired();

            builder.Property(s => s.EmergencyContactName)
                .HasMaxLength(200);

            builder.Property(s => s.EmergencyContactPhone)
                .HasMaxLength(20);

            builder.Property(s => s.MedicalConditions)
                .HasMaxLength(1000);

            builder.Property(s => s.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(s => s.CurrentBelt)
                .IsRequired()
                .HasConversion<int>();

            // Relationships
            builder.HasMany(s => s.Attendances)
                .WithOne(a => a.Student)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(s => s.BeltPromotions)
                .WithOne(bp => bp.Student)
                .HasForeignKey(bp => bp.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(s => s.EventRegistrations)
                .WithOne(er => er.Student)
                .HasForeignKey(er => er.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
