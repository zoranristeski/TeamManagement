using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportsTeamManagement.Domain.Entities;

namespace SportsTeamManagement.Infrastructure.Data.Configurations
{
    public class EventRegistrationConfiguration : IEntityTypeConfiguration<EventRegistration>
    {
        public void Configure(EntityTypeBuilder<EventRegistration> builder)
        {
            builder.HasKey(er => er.Id);

            builder.Property(er => er.StudentId)
                .IsRequired();

            builder.Property(er => er.EventId)
                .IsRequired();

            builder.Property(er => er.RegistrationDate)
                .IsRequired();

            builder.Property(er => er.Notes)
                .HasMaxLength(500);

            builder.Property(er => er.AmountPaid)
                .HasColumnType("decimal(10,2)");

            // Composite index to prevent duplicate registrations
            builder.HasIndex(er => new { er.StudentId, er.EventId })
                .IsUnique();
        }
    }
}
