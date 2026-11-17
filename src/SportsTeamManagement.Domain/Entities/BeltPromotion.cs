using SportsTeamManagement.Domain.Common;
using SportsTeamManagement.Domain.Enums;

namespace SportsTeamManagement.Domain.Entities
{
    public class BeltPromotion : BaseEntity
    {
        public Guid StudentId { get; set; }
        public Guid PromotedById { get; set; }
        public BeltRank FromBelt { get; set; }
        public BeltRank ToBelt { get; set; }
        public DateTime PromotionDate { get; set; }
        public string? Notes { get; set; }
        public string? Requirements { get; set; }

        // Navigation properties
        public Student Student { get; set; } = null!;
        public Trainer PromotedBy { get; set; } = null!;
    }
}
