using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportsTeamManagement.Domain.Entities;

namespace SportsTeamManagement.Infrastructure.Data.Configurations
{
    public class BeltPromotionConfiguration : IEntityTypeConfiguration<BeltPromotion>
    {
        public void Configure(EntityTypeBuilder<BeltPromotion> builder)
        {
            builder.HasKey(bp => bp.Id);

            builder.Property(bp => bp.StudentId)
                .IsRequired();

            builder.Property(bp => bp.PromotedById)
                .IsRequired();

            builder.Property(bp => bp.FromBelt)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(bp => bp.ToBelt)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(bp => bp.PromotionDate)
                .IsRequired();

            builder.Property(bp => bp.Notes)
                .HasMaxLength(1000);

            builder.Property(bp => bp.Requirements)
                .HasMaxLength(2000);
        }
    }
}
