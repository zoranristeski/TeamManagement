using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportsTeamManagement.Domain.Entities;

namespace SportsTeamManagement.Infrastructure.Data.Configurations
{
    public class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
    {
        public void Configure(EntityTypeBuilder<Attendance> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.StudentId)
                .IsRequired();

            builder.Property(a => a.TrainingSessionId)
                .IsRequired();

            builder.Property(a => a.AttendanceDate)
                .IsRequired();

            builder.Property(a => a.Notes)
                .HasMaxLength(500);

            // Composite index to prevent duplicate attendance records
            builder.HasIndex(a => new { a.StudentId, a.TrainingSessionId })
                .IsUnique();
        }
    }
}
