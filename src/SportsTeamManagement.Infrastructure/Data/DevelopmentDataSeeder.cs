using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SportsTeamManagement.Domain.Entities;
using SportsTeamManagement.Domain.Enums;
using SportsTeamManagement.Infrastructure.Services;

namespace SportsTeamManagement.Infrastructure.Data;

public static class DevelopmentDataSeeder
{
    private static readonly Guid IkigaiClubId = Guid.Parse("a0000000-0000-0000-0000-000000000001");
    private static readonly Guid VardarClubId = Guid.Parse("a0000000-0000-0000-0000-000000000002");
    private static readonly Guid ChampionsClubId = Guid.Parse("a0000000-0000-0000-0000-000000000003");

    private static readonly Guid AdminUserId = Guid.Parse("b0000000-0000-0000-0000-000000000001");
    private static readonly Guid Trainer1UserId = Guid.Parse("b0000000-0000-0000-0000-000000000002");
    private static readonly Guid Trainer2UserId = Guid.Parse("b0000000-0000-0000-0000-000000000101");
    private static readonly Guid Trainer3UserId = Guid.Parse("b0000000-0000-0000-0000-000000000102");
    private static readonly Guid Student1UserId = Guid.Parse("b0000000-0000-0000-0000-000000000003");
    private static readonly Guid Student2UserId = Guid.Parse("b0000000-0000-0000-0000-000000000111");
    private static readonly Guid Student3UserId = Guid.Parse("b0000000-0000-0000-0000-000000000112");
    private static readonly Guid Student4UserId = Guid.Parse("b0000000-0000-0000-0000-000000000113");
    private static readonly Guid Student5UserId = Guid.Parse("b0000000-0000-0000-0000-000000000114");
    private static readonly Guid ClubAdminUserId = Guid.Parse("b0000000-0000-0000-0000-000000000121");
    private static readonly Guid OfficeStaffUserId = Guid.Parse("b0000000-0000-0000-0000-000000000122");
    private static readonly Guid ParentUserId = Guid.Parse("b0000000-0000-0000-0000-000000000004");

    public static async Task SeedAsync(
        ApplicationDbContext db,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var connectionString = db.Database.GetConnectionString()
            ?? throw new InvalidOperationException("A SQL Server connection string is required to seed development data.");
        var dataSource = new SqlConnectionStringBuilder(connectionString).DataSource;
        if (!IsLocalDataSource(dataSource))
            throw new InvalidOperationException("Development sample data can only be seeded into a local SQL Server or LocalDB instance.");

        await db.Database.MigrateAsync(cancellationToken);

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var now = DateTime.UtcNow;

        await UpsertClub(db, IkigaiClubId, "IKIGAI Martial Arts", "ikigai-martial-arts",
            "https://example.invalid/ikigai", "Skopje - Center", true, cancellationToken);
        await UpsertClub(db, VardarClubId, "Vardar Sports Club", "vardar-sports-club",
            "https://example.invalid/vardar", "Skopje - Aerodrom", false, cancellationToken);
        await UpsertClub(db, ChampionsClubId, "Champions Academy", "champions-academy",
            "https://example.invalid/champions", "Tetovo - Sports Campus", true, cancellationToken);

        var admin = await UpsertUser(db, AdminUserId, "dev-seed:admin", "admin@test.com",
            "Alex", "Morgan", UserRole.PlatformAdmin, "Admin123!", cancellationToken);
        var trainer1 = await UpsertUser(db, Trainer1UserId, "dev-seed:trainer1", "trainer1@test.com",
            "Elena", "Ristova", UserRole.Trainer, "Test123!", cancellationToken);
        var trainer2 = await UpsertUser(db, Trainer2UserId, "dev-seed:trainer2", "trainer2@test.com",
            "Marko", "Petrov", UserRole.Trainer, "Test123!", cancellationToken);
        var trainer3 = await UpsertUser(db, Trainer3UserId, "dev-seed:trainer3", "trainer3@test.com",
            "Sara", "Ilic", UserRole.Trainer, "Test123!", cancellationToken);
        var clubAdmin = await UpsertUser(db, ClubAdminUserId, "dev-seed:clubadmin", "clubadmin@test.com",
            "Nina", "Stojanova", UserRole.ClubAdmin, "Test123!", cancellationToken);
        var officeStaff = await UpsertUser(db, OfficeStaffUserId, "dev-seed:officestaff", "officestaff@test.com",
            "Bojan", "Trajkov", UserRole.OfficeStaff, "Test123!", cancellationToken);
        var parent = await UpsertUser(db, ParentUserId, "dev-seed:parent", "parent@test.com",
            "Ana", "Petrova", UserRole.Parent, "Test123!", cancellationToken);

        var student1 = await UpsertUser(db, Student1UserId, "dev-seed:student1", "student1@test.com",
            "Elena", "Petrova", UserRole.Student, "Test123!", cancellationToken);
        var student2 = await UpsertUser(db, Student2UserId, "dev-seed:student2", "student2@test.com",
            "Mark", "Jovanov", UserRole.Student, "Test123!", cancellationToken);
        var student3 = await UpsertUser(db, Student3UserId, "dev-seed:student3", "student3@test.com",
            "Ana", "Kovaceva", UserRole.Student, "Test123!", cancellationToken);
        var student4 = await UpsertUser(db, Student4UserId, "dev-seed:student4", "student4@test.com",
            "Luka", "Dimitrov", UserRole.Student, "Test123!", cancellationToken);
        var student5 = await UpsertUser(db, Student5UserId, "dev-seed:student5", "student5@test.com",
            "Maya", "Angelova", UserRole.Student, "Test123!", cancellationToken);

        await UpsertClubMembership(db, Guid.Parse("c1000000-0000-0000-0000-000000000001"),
            AdminUserId, IkigaiClubId, UserRole.PlatformAdmin, admin.Id, now, cancellationToken);
        await UpsertClubMembership(db, Guid.Parse("c1000000-0000-0000-0000-000000000101"),
            AdminUserId, VardarClubId, UserRole.PlatformAdmin, admin.Id, now, cancellationToken);
        await UpsertClubMembership(db, Guid.Parse("c1000000-0000-0000-0000-000000000102"),
            AdminUserId, ChampionsClubId, UserRole.PlatformAdmin, admin.Id, now, cancellationToken);
        await UpsertClubMembership(db, Guid.Parse("c1000000-0000-0000-0000-000000000002"),
            Trainer1UserId, IkigaiClubId, UserRole.Trainer, admin.Id, now, cancellationToken);
        await UpsertClubMembership(db, Guid.Parse("c1000000-0000-0000-0000-000000000005"),
            Trainer2UserId, VardarClubId, UserRole.Trainer, admin.Id, now, cancellationToken);
        await UpsertClubMembership(db, Guid.Parse("c1000000-0000-0000-0000-000000000006"),
            Trainer3UserId, ChampionsClubId, UserRole.Trainer, admin.Id, now, cancellationToken);
        await UpsertClubMembership(db, Guid.Parse("c1000000-0000-0000-0000-000000000007"),
            ClubAdminUserId, IkigaiClubId, UserRole.ClubAdmin, admin.Id, now, cancellationToken);
        await UpsertClubMembership(db, Guid.Parse("c1000000-0000-0000-0000-000000000008"),
            OfficeStaffUserId, VardarClubId, UserRole.OfficeStaff, admin.Id, now, cancellationToken);
        await UpsertClubMembership(db, Guid.Parse("c1000000-0000-0000-0000-000000000004"),
            ParentUserId, IkigaiClubId, UserRole.Parent, admin.Id, now, cancellationToken);

        await UpsertClubMembership(db, Guid.Parse("c1000000-0000-0000-0000-000000000003"),
            Student1UserId, IkigaiClubId, UserRole.Student, admin.Id, now, cancellationToken);
        await UpsertClubMembership(db, Guid.Parse("c1000000-0000-0000-0000-000000000032"),
            Student2UserId, IkigaiClubId, UserRole.Student, admin.Id, now, cancellationToken);
        await UpsertClubMembership(db, Guid.Parse("c1000000-0000-0000-0000-000000000033"),
            Student3UserId, VardarClubId, UserRole.Student, admin.Id, now, cancellationToken);
        await UpsertClubMembership(db, Guid.Parse("c1000000-0000-0000-0000-000000000034"),
            Student4UserId, VardarClubId, UserRole.Student, admin.Id, now, cancellationToken);
        await UpsertClubMembership(db, Guid.Parse("c1000000-0000-0000-0000-000000000035"),
            Student5UserId, ChampionsClubId, UserRole.Student, admin.Id, now, cancellationToken);

        var trainerProfiles = new[]
        {
            (UserId: Trainer1UserId, Id: Guid.Parse("c0000000-0000-0000-0000-000000000002"), Rank: BeltRank.Black2nd, Specialty: "Aikido", Club: "IKIGAI"),
            (UserId: Trainer2UserId, Id: Guid.Parse("c2000000-0000-0000-0000-000000000002"), Rank: BeltRank.Black1st, Specialty: "Athletic conditioning", Club: "Vardar"),
            (UserId: Trainer3UserId, Id: Guid.Parse("c2000000-0000-0000-0000-000000000003"), Rank: BeltRank.Black3rd, Specialty: "Performance training", Club: "Champions")
        };
        foreach (var profile in trainerProfiles)
            await UpsertTrainer(db, profile.Id, profile.UserId, profile.Rank, profile.Specialty,
                now.AddYears(-profile.Id.ToString()[^1].ToString().Length), $"Certified {profile.Specialty} coach.", cancellationToken);

        var studentProfiles = new[]
        {
            (UserId: Student1UserId, Id: Guid.Parse("c0000000-0000-0000-0000-000000000001"), Dob: new DateTime(2012, 3, 15), Belt: BeltRank.Yellow),
            (UserId: Student2UserId, Id: Guid.Parse("c3000000-0000-0000-0000-000000000002"), Dob: new DateTime(2010, 9, 22), Belt: BeltRank.Orange),
            (UserId: Student3UserId, Id: Guid.Parse("c3000000-0000-0000-0000-000000000003"), Dob: new DateTime(2013, 1, 10), Belt: BeltRank.White),
            (UserId: Student4UserId, Id: Guid.Parse("c3000000-0000-0000-0000-000000000004"), Dob: new DateTime(2011, 6, 18), Belt: BeltRank.Yellow),
            (UserId: Student5UserId, Id: Guid.Parse("c3000000-0000-0000-0000-000000000005"), Dob: new DateTime(2009, 11, 30), Belt: BeltRank.Green)
        };
        foreach (var profile in studentProfiles)
            await UpsertStudent(db, profile.Id, profile.UserId, profile.Dob, profile.Belt, now, cancellationToken);

        await UpsertMembershipPlans(db, now, cancellationToken);
        await UpsertTrainingGroups(db, now, cancellationToken);
        await UpsertTrainingSessions(db, now, cancellationToken);
        await UpsertAttendance(db, now, cancellationToken);
        await UpsertGuardianData(db, now, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        logger.LogInformation("Development sample clubs, users, memberships, and training data were synchronized.");
    }

    private static async Task UpsertClub(ApplicationDbContext db, Guid id, string name, string slug,
        string website, string location, bool beltGrading, CancellationToken cancellationToken)
    {
        var club = await db.Clubs.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (club is null)
        {
            club = new Club { Id = id };
            db.Clubs.Add(club);
        }
        club.Name = name;
        club.Slug = slug;
        club.WebsiteUrl = website;
        club.Location = location;
        club.TimeZone = "Europe/Skopje";
        club.BeltGradingEnabled = beltGrading;
        club.IsActive = true;
    }

    private static async Task<User> UpsertUser(ApplicationDbContext db, Guid id, string auth0Id, string email,
        string firstName, string lastName, UserRole role, string password, CancellationToken cancellationToken)
    {
        var normalizedEmail = email.ToUpperInvariant();
        var user = await db.Users.SingleOrDefaultAsync(x => x.Id == id || x.NormalizedEmail == normalizedEmail, cancellationToken);
        if (user is null)
        {
            user = new User { Id = id };
            db.Users.Add(user);
        }
        user.Auth0Id = auth0Id;
        user.Email = email;
        user.NormalizedEmail = normalizedEmail;
        user.FirstName = firstName;
        user.LastName = lastName;
        user.PasswordHash = PasswordHashing.Hash(password);
        user.EmailVerified = true;
        user.Role = role;
        user.IsActive = true;
        user.LockoutEndAt = null;
        user.AccessFailedCount = 0;
        return user;
    }

    private static async Task UpsertClubMembership(ApplicationDbContext db, Guid id, Guid userId,
        Guid clubId, UserRole role, Guid approvedBy, DateTime approvedAt, CancellationToken cancellationToken)
    {
        var membership = await db.ClubMemberships
            .SingleOrDefaultAsync(x => x.Id == id || (x.ClubId == clubId && x.UserId == userId), cancellationToken);
        if (membership is null)
        {
            membership = new ClubMembership { Id = id };
            db.ClubMemberships.Add(membership);
        }
        membership.ClubId = clubId;
        membership.UserId = userId;
        membership.ClubRole = role;
        membership.Status = MembershipStatus.Active;
        membership.IsActive = true;
        membership.ApprovedAt = approvedAt;
        membership.ApprovedByUserId = approvedBy;
        membership.SuspensionReason = null;
    }

    private static async Task UpsertTrainer(ApplicationDbContext db, Guid id, Guid userId, BeltRank rank,
        string specialization, DateTime startDate, string biography, CancellationToken cancellationToken)
    {
        var trainer = await db.Trainers.SingleOrDefaultAsync(x => x.Id == id || x.UserId == userId, cancellationToken);
        if (trainer is null)
        {
            trainer = new Trainer { Id = id };
            db.Trainers.Add(trainer);
        }
        trainer.UserId = userId;
        trainer.Rank = rank;
        trainer.Specialization = specialization;
        trainer.StartDate = startDate;
        trainer.Biography = biography;
        trainer.CanApproveStudents = true;
        trainer.CanPromoteBelts = true;
    }

    private static async Task UpsertStudent(ApplicationDbContext db, Guid id, Guid userId, DateTime dob,
        BeltRank belt, DateTime now, CancellationToken cancellationToken)
    {
        var student = await db.Students.SingleOrDefaultAsync(x => x.Id == id || x.UserId == userId, cancellationToken);
        if (student is null)
        {
            student = new Student { Id = id };
            db.Students.Add(student);
        }
        student.UserId = userId;
        student.DateOfBirth = dob;
        student.EmergencyContactName = "Ana Petrova";
        student.EmergencyContactPhone = "+389 70 123 456";
        student.MedicalConditions = "None reported";
        student.JoinDate = now.AddMonths(-10);
        student.Status = StudentStatus.Active;
        student.CurrentBelt = belt;
        student.LastBeltPromotion = now.AddMonths(-3);
        student.NextGradingEligible = now.AddMonths(3);
    }

    private static async Task UpsertMembershipPlans(ApplicationDbContext db, DateTime now, CancellationToken cancellationToken)
    {
        var plans = new[]
        {
            (Id: Guid.Parse("f0000000-0000-0000-0000-000000000001"), ClubId: IkigaiClubId, Name: "Aikido Monthly", Price: 40m),
            (Id: Guid.Parse("f0000000-0000-0000-0000-000000000002"), ClubId: VardarClubId, Name: "Vardar Youth Monthly", Price: 35m),
            (Id: Guid.Parse("f0000000-0000-0000-0000-000000000003"), ClubId: ChampionsClubId, Name: "Performance Monthly", Price: 45m)
        };
        foreach (var planData in plans)
        {
            var plan = await db.MembershipPlans.SingleOrDefaultAsync(x => x.Id == planData.Id, cancellationToken);
            if (plan is null) { plan = new MembershipPlan { Id = planData.Id }; db.MembershipPlans.Add(plan); }
            plan.ClubId = planData.ClubId;
            plan.Name = planData.Name;
            plan.Description = "Development test membership plan.";
            plan.MonthlyPrice = planData.Price;
            plan.IsActive = true;
        }

        var memberships = new[]
        {
            (Id: Guid.Parse("f1000000-0000-0000-0000-000000000001"), ClubId: IkigaiClubId, StudentId: Guid.Parse("c0000000-0000-0000-0000-000000000001"), PlanId: plans[0].Id),
            (Id: Guid.Parse("f1000000-0000-0000-0000-000000000002"), ClubId: IkigaiClubId, StudentId: Guid.Parse("c3000000-0000-0000-0000-000000000002"), PlanId: plans[0].Id),
            (Id: Guid.Parse("f1000000-0000-0000-0000-000000000003"), ClubId: VardarClubId, StudentId: Guid.Parse("c3000000-0000-0000-0000-000000000003"), PlanId: plans[1].Id),
            (Id: Guid.Parse("f1000000-0000-0000-0000-000000000004"), ClubId: VardarClubId, StudentId: Guid.Parse("c3000000-0000-0000-0000-000000000004"), PlanId: plans[1].Id),
            (Id: Guid.Parse("f1000000-0000-0000-0000-000000000005"), ClubId: ChampionsClubId, StudentId: Guid.Parse("c3000000-0000-0000-0000-000000000005"), PlanId: plans[2].Id)
        };
        foreach (var data in memberships)
        {
            var membership = await db.Memberships.SingleOrDefaultAsync(x => x.Id == data.Id, cancellationToken);
            if (membership is null) { membership = new Membership { Id = data.Id }; db.Memberships.Add(membership); }
            membership.ClubId = data.ClubId;
            membership.StudentId = data.StudentId;
            membership.MembershipPlanId = data.PlanId;
            membership.StartDate = now.AddMonths(-6);
            membership.EndDate = now.AddMonths(6);
            membership.Status = MembershipStatus.Active;
        }
    }

    private static async Task UpsertTrainingGroups(ApplicationDbContext db, DateTime now, CancellationToken cancellationToken)
    {
        var groups = new[]
        {
            (Id: Guid.Parse("d1000000-0000-0000-0000-000000000001"), ClubId: IkigaiClubId, TrainerId: Guid.Parse("c0000000-0000-0000-0000-000000000002"), Name: "Aikido Beginners", Sport: "Aikido", Level: "Beginner", Location: "IKIGAI Main Dojo"),
            (Id: Guid.Parse("d1000000-0000-0000-0000-000000000002"), ClubId: IkigaiClubId, TrainerId: Guid.Parse("c0000000-0000-0000-0000-000000000002"), Name: "Aikido Advanced", Sport: "Aikido", Level: "Advanced", Location: "IKIGAI Main Dojo"),
            (Id: Guid.Parse("d1000000-0000-0000-0000-000000000003"), ClubId: VardarClubId, TrainerId: Guid.Parse("c2000000-0000-0000-0000-000000000002"), Name: "General Fitness", Sport: "Fitness", Level: "All levels", Location: "Vardar Gym"),
            (Id: Guid.Parse("d1000000-0000-0000-0000-000000000004"), ClubId: VardarClubId, TrainerId: Guid.Parse("c2000000-0000-0000-0000-000000000002"), Name: "Youth Training", Sport: "Athletics", Level: "Beginner", Location: "Vardar Field"),
            (Id: Guid.Parse("d1000000-0000-0000-0000-000000000005"), ClubId: ChampionsClubId, TrainerId: Guid.Parse("c2000000-0000-0000-0000-000000000003"), Name: "Performance Training", Sport: "Performance sports", Level: "Intermediate", Location: "Champions Arena")
        };
        foreach (var data in groups)
        {
            var group = await db.TrainingGroups.SingleOrDefaultAsync(x => x.Id == data.Id, cancellationToken);
            if (group is null) { group = new TrainingGroup { Id = data.Id }; db.TrainingGroups.Add(group); }
            group.ClubId = data.ClubId;
            group.Name = data.Name;
            group.Description = $"Development test group for {data.Name}.";
            group.SportOrDiscipline = data.Sport;
            group.MinimumAge = 8;
            group.MaximumAge = 18;
            group.SkillLevel = data.Level;
            group.Capacity = 20;
            group.Location = data.Location;
            group.IsActive = true;

            var assignment = await db.TrainingGroupTrainers
                .SingleOrDefaultAsync(x => x.TrainingGroupId == data.Id && x.TrainerId == data.TrainerId, cancellationToken);
            if (assignment is null)
            {
                assignment = new TrainingGroupTrainer
                {
                    Id = Guid.Parse($"d2000000-0000-0000-0000-{data.Id.ToString()[^12..]}"),
                    TrainingGroupId = data.Id,
                    TrainerId = data.TrainerId
                };
                db.TrainingGroupTrainers.Add(assignment);
            }
            assignment.IsActive = true;
        }

        var enrollments = new[]
        {
            (Id: Guid.Parse("d3000000-0000-0000-0000-000000000001"), GroupId: groups[0].Id, StudentId: Guid.Parse("c0000000-0000-0000-0000-000000000001")),
            (Id: Guid.Parse("d3000000-0000-0000-0000-000000000002"), GroupId: groups[1].Id, StudentId: Guid.Parse("c3000000-0000-0000-0000-000000000002")),
            (Id: Guid.Parse("d3000000-0000-0000-0000-000000000003"), GroupId: groups[2].Id, StudentId: Guid.Parse("c3000000-0000-0000-0000-000000000003")),
            (Id: Guid.Parse("d3000000-0000-0000-0000-000000000004"), GroupId: groups[3].Id, StudentId: Guid.Parse("c3000000-0000-0000-0000-000000000004")),
            (Id: Guid.Parse("d3000000-0000-0000-0000-000000000005"), GroupId: groups[4].Id, StudentId: Guid.Parse("c3000000-0000-0000-0000-000000000005"))
        };
        foreach (var data in enrollments)
        {
            var enrollment = await db.TrainingGroupStudents
                .SingleOrDefaultAsync(x => x.Id == data.Id || (x.TrainingGroupId == data.GroupId && x.StudentId == data.StudentId), cancellationToken);
            if (enrollment is null) { enrollment = new TrainingGroupStudent { Id = data.Id }; db.TrainingGroupStudents.Add(enrollment); }
            enrollment.TrainingGroupId = data.GroupId;
            enrollment.StudentId = data.StudentId;
            enrollment.EnrolledAt = now.AddMonths(-6);
            enrollment.IsActive = true;
        }
    }

    private static async Task UpsertTrainingSessions(ApplicationDbContext db, DateTime now, CancellationToken cancellationToken)
    {
        var sessions = new[]
        {
            (Id: Guid.Parse("e1000000-0000-0000-0000-000000000001"), Name: "Aikido Fundamentals", TrainerId: Guid.Parse("c0000000-0000-0000-0000-000000000002"), Offset: -7),
            (Id: Guid.Parse("e1000000-0000-0000-0000-000000000002"), Name: "Aikido Advanced Techniques", TrainerId: Guid.Parse("c0000000-0000-0000-0000-000000000002"), Offset: -3),
            (Id: Guid.Parse("e1000000-0000-0000-0000-000000000003"), Name: "Vardar Strength Session", TrainerId: Guid.Parse("c2000000-0000-0000-0000-000000000002"), Offset: -6),
            (Id: Guid.Parse("e1000000-0000-0000-0000-000000000004"), Name: "Vardar Youth Athletics", TrainerId: Guid.Parse("c2000000-0000-0000-0000-000000000002"), Offset: -2),
            (Id: Guid.Parse("e1000000-0000-0000-0000-000000000005"), Name: "Champions Speed and Agility", TrainerId: Guid.Parse("c2000000-0000-0000-0000-000000000003"), Offset: -4)
        };
        foreach (var data in sessions)
        {
            var session = await db.TrainingSessions.SingleOrDefaultAsync(x => x.Id == data.Id, cancellationToken);
            if (session is null) { session = new TrainingSession { Id = data.Id }; db.TrainingSessions.Add(session); }
            session.Name = data.Name;
            session.Description = "Development test training session.";
            session.StartTime = now.AddDays(data.Offset).Date.AddHours(16);
            session.EndTime = now.AddDays(data.Offset).Date.AddHours(17).AddMinutes(30);
            session.TrainerId = data.TrainerId;
            session.MaxParticipants = 20;
            session.IsActive = true;
        }
    }

    private static async Task UpsertAttendance(ApplicationDbContext db, DateTime now, CancellationToken cancellationToken)
    {
        var records = new[]
        {
            (Id: Guid.Parse("d4000000-0000-0000-0000-000000000001"), StudentId: Guid.Parse("c0000000-0000-0000-0000-000000000001"), SessionId: Guid.Parse("e1000000-0000-0000-0000-000000000001"), Offset: -7),
            (Id: Guid.Parse("d4000000-0000-0000-0000-000000000002"), StudentId: Guid.Parse("c3000000-0000-0000-0000-000000000002"), SessionId: Guid.Parse("e1000000-0000-0000-0000-000000000002"), Offset: -3),
            (Id: Guid.Parse("d4000000-0000-0000-0000-000000000003"), StudentId: Guid.Parse("c3000000-0000-0000-0000-000000000003"), SessionId: Guid.Parse("e1000000-0000-0000-0000-000000000003"), Offset: -6),
            (Id: Guid.Parse("d4000000-0000-0000-0000-000000000004"), StudentId: Guid.Parse("c3000000-0000-0000-0000-000000000004"), SessionId: Guid.Parse("e1000000-0000-0000-0000-000000000004"), Offset: -2),
            (Id: Guid.Parse("d4000000-0000-0000-0000-000000000005"), StudentId: Guid.Parse("c3000000-0000-0000-0000-000000000005"), SessionId: Guid.Parse("e1000000-0000-0000-0000-000000000005"), Offset: -4)
        };
        foreach (var data in records)
        {
            var attendance = await db.Attendances.SingleOrDefaultAsync(x => x.Id == data.Id, cancellationToken);
            if (attendance is null) { attendance = new Attendance { Id = data.Id }; db.Attendances.Add(attendance); }
            attendance.StudentId = data.StudentId;
            attendance.TrainingSessionId = data.SessionId;
            attendance.AttendanceDate = now.AddDays(data.Offset).Date;
            attendance.IsPresent = true;
            attendance.Notes = "Development seed attendance.";
        }
    }

    private static async Task UpsertGuardianData(ApplicationDbContext db, DateTime now, CancellationToken cancellationToken)
    {
        var guardianId = Guid.Parse("f0000000-0000-0000-0000-000000000004");
        var guardian = await db.Guardians.SingleOrDefaultAsync(x => x.Id == guardianId, cancellationToken);
        if (guardian is null) { guardian = new Guardian { Id = guardianId }; db.Guardians.Add(guardian); }
        guardian.ClubId = IkigaiClubId;
        guardian.UserId = ParentUserId;
        guardian.Relationship = "Parent";
        guardian.IsEmergencyContact = true;
        guardian.CanViewMedicalInformation = true;

        var relationshipId = Guid.Parse("d0000000-0000-0000-0000-000000000009");
        var relationship = await db.GuardianStudents.SingleOrDefaultAsync(x => x.Id == relationshipId, cancellationToken);
        if (relationship is null) { relationship = new GuardianStudent { Id = relationshipId }; db.GuardianStudents.Add(relationship); }
        relationship.GuardianId = guardianId;
        relationship.StudentId = Guid.Parse("c0000000-0000-0000-0000-000000000001");
        relationship.IsAuthorized = true;

        var parentRelationshipId = Guid.Parse("d0000000-0000-0000-0000-000000000014");
        var parentRelationship = await db.ParentStudentRelationships
            .SingleOrDefaultAsync(x => x.Id == parentRelationshipId, cancellationToken);
        if (parentRelationship is null)
        {
            parentRelationship = new ParentStudentRelationship { Id = parentRelationshipId };
            db.ParentStudentRelationships.Add(parentRelationship);
        }
        parentRelationship.ParentUserId = ParentUserId;
        parentRelationship.StudentUserId = Student1UserId;
        parentRelationship.ClubId = IkigaiClubId;
        parentRelationship.VerificationStatus = RelationshipVerificationStatus.Verified;
        parentRelationship.RelationshipType = "Parent";
        parentRelationship.VerifiedAt = now.AddMonths(-6);
        parentRelationship.VerifiedByUserId = AdminUserId;
        parentRelationship.IsActive = true;
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
            return true;

        var hostWithoutInstance = host.Split('\\', ',')[0].Trim('[', ']');
        return System.Net.IPAddress.TryParse(hostWithoutInstance, out var address) &&
            System.Net.IPAddress.IsLoopback(address);
    }
}
