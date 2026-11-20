using Microsoft.EntityFrameworkCore;
using SportsTeamManagement.Domain.Entities;
using SportsTeamManagement.Domain.Common;

namespace SportsTeamManagement.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Trainer> Trainers { get; set; }
        public DbSet<TrainingSession> TrainingSessions { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<BeltPromotion> BeltPromotions { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventRegistration> EventRegistrations { get; set; }
        public DbSet<Club> Clubs { get; set; }
        public DbSet<ClubMembership> ClubMemberships { get; set; }
        public DbSet<TrainingGroup> TrainingGroups { get; set; }
        public DbSet<TrainingGroupStudent> TrainingGroupStudents { get; set; }
        public DbSet<TrainingGroupTrainer> TrainingGroupTrainers { get; set; }
        public DbSet<Guardian> Guardians { get; set; }
        public DbSet<GuardianStudent> GuardianStudents { get; set; }
        public DbSet<MembershipPlan> MembershipPlans { get; set; }
        public DbSet<Membership> Memberships { get; set; }
        public DbSet<PaymentRecord> PaymentRecords { get; set; }
        public DbSet<ExternalLogin> ExternalLogins { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<EnrollmentRequest> EnrollmentRequests { get; set; }
        public DbSet<ParentStudentRelationship> ParentStudentRelationships { get; set; }
        public DbSet<RoleAssignmentHistory> RoleAssignmentHistories { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all configurations from the current assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}