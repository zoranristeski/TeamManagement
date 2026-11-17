using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportsTeamManagement.Domain.Entities;

namespace SportsTeamManagement.Infrastructure.Data.Configurations
{
    public class TrainingSessionConfiguration : IEntityTypeConfiguration<TrainingSession>
    {
        public void Configure(EntityTypeBuilder<TrainingSession> builder)
        {
            builder.HasKey(ts => ts.Id);

            builder.Property(ts => ts.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(ts => ts.Description)
                .HasMaxLength(1000);

            builder.Property(ts => ts.StartTime)
                .IsRequired();

            builder.Property(ts => ts.EndTime)
                .IsRequired();

            builder.Property(ts => ts.TrainerId)
                .IsRequired();

            builder.Property(ts => ts.MaxParticipants)
                .IsRequired();

            // Relationships
            builder.HasMany(ts => ts.Attendances)
                .WithOne(a => a.TrainingSession)
                .HasForeignKey(a => a.TrainingSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
