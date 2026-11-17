using SportsTeamManagement.Domain.Common;
using SportsTeamManagement.Domain.Enums;

namespace SportsTeamManagement.Domain.Entities
{
    public class Student : BaseEntity
    {
        public Guid UserId { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? MedicalConditions { get; set; }
        public DateTime JoinDate { get; set; } = DateTime.UtcNow;
        public StudentStatus Status { get; set; } = StudentStatus.PendingApproval;
        public BeltRank CurrentBelt { get; set; } = BeltRank.White;
        public DateTime? LastBeltPromotion { get; set; }
        public DateTime? NextGradingEligible { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public ICollection<BeltPromotion> BeltPromotions { get; set; } = new List<BeltPromotion>();
        public ICollection<EventRegistration> EventRegistrations { get; set; } = new List<EventRegistration>();

        public int Age => DateTime.UtcNow.Year - DateOfBirth.Year - 
                         (DateTime.UtcNow.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);
    }
}
