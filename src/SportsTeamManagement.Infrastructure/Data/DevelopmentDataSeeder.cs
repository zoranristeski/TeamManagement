using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SportsTeamManagement.Domain.Entities;
using SportsTeamManagement.Domain.Enums;
using SportsTeamManagement.Infrastructure.Services;

namespace SportsTeamManagement.Infrastructure.Data;

public static class DevelopmentDataSeeder
{
    private static readonly Guid ClubId = Guid.Parse("a0000000-0000-0000-0000-000000000001");
    private static readonly Guid AdminUserId = Guid.Parse("b0000000-0000-0000-0000-000000000001");
    private static readonly Guid TrainerUserId = Guid.Parse("b0000000-0000-0000-0000-000000000002");
    private static readonly Guid StudentUserId = Guid.Parse("b0000000-0000-0000-0000-000000000003");
    private static readonly Guid ParentUserId = Guid.Parse("b0000000-0000-0000-0000-000000000004");
    private static readonly Guid ApplicantUserId = Guid.Parse("b0000000-0000-0000-0000-000000000005");

    public static async Task SeedAsync(
        ApplicationDbContext db,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var connectionString = db.Database.GetConnectionString()
            ?? throw new InvalidOperationException("A SQL Server connection string is required to seed development data.");
        var dataSource = new SqlConnectionStringBuilder(connectionString).DataSource;
        if (!IsLocalDataSource(dataSource))
        {
            throw new InvalidOperationException(
                "Development sample data can only be seeded into a local SQL Server or LocalDB instance.");
        }

        await db.Database.MigrateAsync(cancellationToken);
        if (await db.Users.AnyAsync(x => x.Auth0Id == "dev-seed:admin", cancellationToken))
        {
            logger.LogInformation("Development sample data already exists.");
            return;
        }

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var now = DateTime.UtcNow;
        var studentId = Guid.Parse("c0000000-0000-0000-0000-000000000001");
        var trainerId = Guid.Parse("c0000000-0000-0000-0000-000000000002");
        var groupId = Guid.Parse("d0000000-0000-0000-0000-000000000001");
        var sessionOneId = Guid.Parse("e0000000-0000-0000-0000-000000000001");
        var sessionTwoId = Guid.Parse("e0000000-0000-0000-0000-000000000002");
        var membershipPlanId = Guid.Parse("f0000000-0000-0000-0000-000000000001");
        var membershipId = Guid.Parse("f0000000-0000-0000-0000-000000000002");
        var eventId = Guid.Parse("f0000000-0000-0000-0000-000000000003");
        var guardianId = Guid.Parse("f0000000-0000-0000-0000-000000000004");

        db.Users.AddRange(
            CreateUser(AdminUserId, "dev-seed:admin", "demo.admin@example.invalid", "Demo", "Administrator",
                UserRole.PlatformAdmin, "DemoAdmin!123"),
            CreateUser(TrainerUserId, "dev-seed:trainer", "demo.trainer@example.invalid", "Taylor", "Trainer", UserRole.Student),
            CreateUser(StudentUserId, "dev-seed:student", "demo.student@example.invalid", "Sam", "Student", UserRole.Student),
            CreateUser(ParentUserId, "dev-seed:parent", "demo.parent@example.invalid", "Pat", "Parent", UserRole.Student),
            CreateUser(ApplicantUserId, "dev-seed:applicant", "demo.applicant@example.invalid", "Alex", "Applicant", UserRole.Student));

        db.Clubs.Add(new Club
        {
            Id = ClubId,
            Name = "Northstar Martial Arts",
            Slug = "northstar-martial-arts",
            WebsiteUrl = "https://example.invalid/northstar",
            Location = "North District",
            TimeZone = "Europe/Skopje",
            BeltGradingEnabled = true,
            IsActive = true
        });
        db.Trainers.Add(new Trainer
        {
            Id = trainerId,
            UserId = TrainerUserId,
            Rank = BeltRank.Black1st,
            Specialization = "Karate",
            StartDate = now.AddYears(-5),
            Biography = "Development sample trainer.",
            CanApproveStudents = true,
            CanPromoteBelts = true
        });
        db.Students.Add(new Student
        {
            Id = studentId,
            UserId = StudentUserId,
            DateOfBirth = now.AddYears(-14),
            EmergencyContactName = "Demo Contact",
            EmergencyContactPhone = "+38970000000",
            MedicalConditions = "None",
            JoinDate = now.AddMonths(-8),
            Status = StudentStatus.Active,
            CurrentBelt = BeltRank.Yellow,
            LastBeltPromotion = now.AddMonths(-3),
            NextGradingEligible = now.AddMonths(3)
        });

        db.ClubMemberships.AddRange(
            CreateClubMembership(AdminUserId, UserRole.ClubAdmin, AdminUserId, now),
            CreateClubMembership(TrainerUserId, UserRole.Trainer, AdminUserId, now),
            CreateClubMembership(StudentUserId, UserRole.Student, AdminUserId, now),
            CreateClubMembership(ParentUserId, UserRole.Parent, AdminUserId, now));
        db.TrainingGroups.Add(new TrainingGroup
        {
            Id = groupId,
            ClubId = ClubId,
            Name = "Youth Karate",
            Description = "Development sample training group.",
            SportOrDiscipline = "Karate",
            MinimumAge = 10,
            MaximumAge = 17,
            SkillLevel = "Beginner",
            Capacity = 20,
            Location = "Main Dojo",
            IsActive = true
        });
        db.TrainingGroupStudents.Add(new TrainingGroupStudent
        {
            Id = Guid.Parse("d0000000-0000-0000-0000-000000000002"),
            TrainingGroupId = groupId,
            StudentId = studentId,
            EnrolledAt = now.AddMonths(-8),
            IsActive = true
        });
        db.TrainingGroupTrainers.Add(new TrainingGroupTrainer
        {
            Id = Guid.Parse("d0000000-0000-0000-0000-000000000003"),
            TrainingGroupId = groupId,
            TrainerId = trainerId,
            IsActive = true
        });

        db.TrainingSessions.AddRange(
            new TrainingSession
            {
                Id = sessionOneId,
                Name = "Youth Karate - Fundamentals",
                Description = "Stances, movement, and basic combinations.",
                StartTime = now.AddDays(-7).Date.AddHours(16),
                EndTime = now.AddDays(-7).Date.AddHours(17),
                TrainerId = trainerId,
                MaxParticipants = 20,
                IsActive = true
            },
            new TrainingSession
            {
                Id = sessionTwoId,
                Name = "Youth Karate - Kata",
                Description = "Practice and review of beginner kata.",
                StartTime = now.AddDays(-3).Date.AddHours(16),
                EndTime = now.AddDays(-3).Date.AddHours(17),
                TrainerId = trainerId,
                MaxParticipants = 20,
                IsActive = true
            });
        db.Attendances.AddRange(
            new Attendance
            {
                Id = Guid.Parse("d0000000-0000-0000-0000-000000000004"),
                StudentId = studentId,
                TrainingSessionId = sessionOneId,
                AttendanceDate = now.AddDays(-7).Date,
                IsPresent = true,
                Notes = "On time."
            },
            new Attendance
            {
                Id = Guid.Parse("d0000000-0000-0000-0000-000000000005"),
                StudentId = studentId,
                TrainingSessionId = sessionTwoId,
                AttendanceDate = now.AddDays(-3).Date,
                IsPresent = true,
                Notes = "Participated in all drills."
            });
        db.BeltPromotions.Add(new BeltPromotion
        {
            Id = Guid.Parse("d0000000-0000-0000-0000-000000000006"),
            StudentId = studentId,
            PromotedById = trainerId,
            FromBelt = BeltRank.White,
            ToBelt = BeltRank.Yellow,
            PromotionDate = now.AddMonths(-3),
            Notes = "Development sample promotion.",
            Requirements = "Completed the beginner grading."
        });

        db.Events.Add(new Event
        {
            Id = eventId,
            Name = "Spring Karate Open",
            Description = "Development sample club event.",
            Type = EventType.Competition,
            StartDate = now.AddMonths(1),
            EndDate = now.AddMonths(1).AddHours(5),
            Location = "North District Sports Hall",
            MaxParticipants = 40,
            Cost = 15,
            IsActive = true,
            RegistrationDeadline = now.AddDays(20),
            OrganizerId = trainerId
        });
        db.EventRegistrations.Add(new EventRegistration
        {
            Id = Guid.Parse("d0000000-0000-0000-0000-000000000007"),
            StudentId = studentId,
            EventId = eventId,
            RegistrationDate = now.AddDays(-2),
            IsConfirmed = true,
            IsAttended = false,
            Notes = "Development sample registration.",
            AmountPaid = 15
        });

        db.MembershipPlans.Add(new MembershipPlan
        {
            Id = membershipPlanId,
            ClubId = ClubId,
            Name = "Youth Monthly",
            Description = "Development sample monthly plan.",
            MonthlyPrice = 35,
            IsActive = true
        });
        db.Memberships.Add(new Membership
        {
            Id = membershipId,
            ClubId = ClubId,
            StudentId = studentId,
            MembershipPlanId = membershipPlanId,
            StartDate = now.AddMonths(-3),
            EndDate = now.AddMonths(9),
            Status = MembershipStatus.Active
        });
        db.PaymentRecords.Add(new PaymentRecord
        {
            Id = Guid.Parse("d0000000-0000-0000-0000-000000000008"),
            ClubId = ClubId,
            MembershipId = membershipId,
            Amount = 35,
            PaymentDate = now.AddDays(-5),
            Status = PaymentStatus.Paid,
            Reference = "DEV-SAMPLE-PAYMENT"
        });

        db.Guardians.Add(new Guardian
        {
            Id = guardianId,
            ClubId = ClubId,
            UserId = ParentUserId,
            Relationship = "Parent",
            IsEmergencyContact = true,
            CanViewMedicalInformation = true
        });
        db.GuardianStudents.Add(new GuardianStudent
        {
            Id = Guid.Parse("d0000000-0000-0000-0000-000000000009"),
            GuardianId = guardianId,
            StudentId = studentId,
            IsAuthorized = true
        });

        db.ExternalLogins.Add(new ExternalLogin
        {
            Id = Guid.Parse("d0000000-0000-0000-0000-000000000010"),
            UserId = AdminUserId,
            Provider = "development-seed",
            ProviderUserId = "demo-admin",
            Email = "demo.admin@example.invalid",
            EmailVerified = true
        });
        db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.Parse("d0000000-0000-0000-0000-000000000011"),
            UserId = AdminUserId,
            TokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("development-seed-invalid-refresh"))).ToLowerInvariant(),
            TokenFamilyId = Guid.Parse("d0000000-0000-0000-0000-000000000012"),
            ExpiresAt = now.AddDays(-1),
            RevokedAt = now,
            RevocationReason = "Development seed placeholder"
        });
        db.EnrollmentRequests.Add(new EnrollmentRequest
        {
            Id = Guid.Parse("d0000000-0000-0000-0000-000000000013"),
            UserId = ApplicantUserId,
            ClubId = ClubId,
            RequestedRole = UserRole.Student,
            Status = EnrollmentRequestStatus.Pending,
            Message = "I would like to join the youth karate group."
        });
        db.ParentStudentRelationships.Add(new ParentStudentRelationship
        {
            Id = Guid.Parse("d0000000-0000-0000-0000-000000000014"),
            ParentUserId = ParentUserId,
            StudentUserId = StudentUserId,
            ClubId = ClubId,
            VerificationStatus = RelationshipVerificationStatus.Verified,
            RelationshipType = "Parent",
            VerifiedAt = now.AddMonths(-7),
            VerifiedByUserId = AdminUserId,
            IsActive = true
        });
        db.RoleAssignmentHistories.Add(new RoleAssignmentHistory
        {
            Id = Guid.Parse("d0000000-0000-0000-0000-000000000015"),
            UserId = TrainerUserId,
            ClubId = ClubId,
            PreviousRole = UserRole.Student,
            NewRole = UserRole.Trainer,
            AssignedByUserId = AdminUserId,
            Reason = "Development sample trainer appointment."
        });
        db.AuditLogs.Add(new AuditLog
        {
            Id = Guid.Parse("d0000000-0000-0000-0000-000000000016"),
            ActorUserId = AdminUserId,
            TargetUserId = TrainerUserId,
            ClubId = ClubId,
            Action = "DevelopmentSeed",
            Details = "Created sample club, memberships, and training records.",
            CorrelationId = "development-seed"
        });

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        logger.LogInformation("Development sample records were created.");
    }

    private static bool IsLocalDataSource(string dataSource)
    {
        var host = dataSource.Trim();
        if (host.StartsWith("(localdb)", StringComparison.OrdinalIgnoreCase) ||
            host.Equals(".", StringComparison.OrdinalIgnoreCase) ||
            host.Equals("(local)", StringComparison.OrdinalIgnoreCase) ||
            host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
            host.StartsWith("localhost\\", StringComparison.OrdinalIgnoreCase) ||
            host.Equals(Environment.MachineName, StringComparison.OrdinalIgnoreCase) ||
            host.StartsWith(Environment.MachineName + "\\", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var hostWithoutInstance = host.Split('\\', ',')[0].Trim('[', ']');
        return System.Net.IPAddress.TryParse(hostWithoutInstance, out var address) &&
            System.Net.IPAddress.IsLoopback(address);
    }

    private static User CreateUser(
        Guid id,
        string auth0Id,
        string email,
        string firstName,
        string lastName,
        UserRole role,
        string? password = null)
    {
        return new User
        {
            Id = id,
            Auth0Id = auth0Id,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            FirstName = firstName,
            LastName = lastName,
            PasswordHash = password is null ? null : PasswordHashing.Hash(password),
            EmailVerified = true,
            Role = role,
            IsActive = true
        };
    }

    private static ClubMembership CreateClubMembership(Guid userId, UserRole role, Guid approvedBy, DateTime approvedAt)
    {
        var suffix = userId.ToString()[^1];
        return new ClubMembership
        {
            Id = Guid.Parse($"c1000000-0000-0000-0000-00000000000{suffix}"),
            ClubId = ClubId,
            UserId = userId,
            ClubRole = role,
            Status = MembershipStatus.Active,
            IsActive = true,
            ApprovedAt = approvedAt,
            ApprovedByUserId = approvedBy
        };
    }
}
