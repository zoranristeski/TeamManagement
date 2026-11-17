using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportsTeamManagement.Domain.Entities;

namespace SportsTeamManagement.Infrastructure.Data.Configurations
{
    public class TrainerConfiguration : IEntityTypeConfiguration<Trainer>
    {
        public void Configure(EntityTypeBuilder<Trainer> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.UserId)
                .IsRequired();

            builder.Property(t => t.Rank)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(t => t.Specialization)
                .HasMaxLength(200);

            builder.Property(t => t.Biography)
                .HasMaxLength(2000);

            // Relationships
            builder.HasMany(t => t.TrainingSessions)
                .WithOne(ts => ts.Trainer)
                .HasForeignKey(ts => ts.TrainerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(t => t.Events)
                .WithOne(e => e.Organizer)
                .HasForeignKey(e => e.OrganizerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(t => t.BeltPromotions)
                .WithOne(bp => bp.PromotedBy)
                .HasForeignKey(bp => bp.PromotedById)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
