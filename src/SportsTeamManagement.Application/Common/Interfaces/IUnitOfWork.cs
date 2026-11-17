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

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}
