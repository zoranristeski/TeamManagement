namespace SportsTeamManagement.Application.Common.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IStudentRepository Students { get; }
        IAttendanceRepository Attendances { get; }
        IRepository<Domain.Entities.Trainer> Trainers { get; }
        IRepository<Domain.Entities.TrainingSession> TrainingSessions { get; }
        IRepository<Domain.Entities.BeltPromotion> BeltPromotions { get; }
        IRepository<Domain.Entities.Event> Events { get; }
        IRepository<Domain.Entities.EventRegistration> EventRegistrations { get; }
        IRepository<Domain.Entities.Club> Clubs { get; }
        IRepository<Domain.Entities.ClubMembership> ClubMemberships { get; }
        IRepository<Domain.Entities.TrainingGroup> TrainingGroups { get; }
        IRepository<Domain.Entities.Guardian> Guardians { get; }
        IRepository<Domain.Entities.MembershipPlan> MembershipPlans { get; }
        IRepository<Domain.Entities.Membership> Memberships { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}
