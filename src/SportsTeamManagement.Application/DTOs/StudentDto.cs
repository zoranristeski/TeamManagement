using SportsTeamManagement.Domain.Enums;

namespace SportsTeamManagement.Application.DTOs
{
    public class StudentDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public UserDto User { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? MedicalConditions { get; set; }
        public DateTime JoinDate { get; set; }
        public StudentStatus Status { get; set; }
        public BeltRank CurrentBelt { get; set; }
        public DateTime? LastBeltPromotion { get; set; }
        public DateTime? NextGradingEligible { get; set; }
        public int Age { get; set; }
        public int TotalAttendances { get; set; }
    }

    public class CreateStudentDto
    {
        public Guid UserId { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? MedicalConditions { get; set; }
    }

    public class UpdateStudentDto
    {
        public DateTime DateOfBirth { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? MedicalConditions { get; set; }
        public StudentStatus Status { get; set; }
        public BeltRank CurrentBelt { get; set; }
        public DateTime? NextGradingEligible { get; set; }
    }

    public class StudentRegistrationDto
    {
        public string Auth0Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? ProfileImageUrl { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? MedicalConditions { get; set; }
    }
}
