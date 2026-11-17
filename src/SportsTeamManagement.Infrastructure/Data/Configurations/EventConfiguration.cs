using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportsTeamManagement.Domain.Entities;

namespace SportsTeamManagement.Infrastructure.Data.Configurations
{
    public class EventConfiguration : IEntityTypeConfiguration<Event>
    {
        public void Configure(EntityTypeBuilder<Event> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.Description)
                .HasMaxLength(2000);

            builder.Property(e => e.Type)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(e => e.StartDate)
                .IsRequired();

            builder.Property(e => e.EndDate)
                .IsRequired();

            builder.Property(e => e.Location)
                .HasMaxLength(300);

            builder.Property(e => e.MaxParticipants)
                .IsRequired();

            builder.Property(e => e.Cost)
                .HasColumnType("decimal(10,2)");

            builder.Property(e => e.RegistrationDeadline)
                .IsRequired();

            builder.Property(e => e.OrganizerId)
                .IsRequired();

            // Relationships
            builder.HasMany(e => e.Registrations)
                .WithOne(er => er.Event)
                .HasForeignKey(er => er.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
