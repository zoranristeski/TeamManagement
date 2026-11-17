using SportsTeamManagement.Domain.Common;
using SportsTeamManagement.Domain.Enums;

namespace SportsTeamManagement.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Auth0Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public UserRole Role { get; set; }
        public bool IsActive { get; set; } = true;
        public string? ProfileImageUrl { get; set; }

        // Navigation properties
        public Student? Student { get; set; }
        public Trainer? Trainer { get; set; }

        public string FullName => $"{FirstName} {LastName}";
    }
}
