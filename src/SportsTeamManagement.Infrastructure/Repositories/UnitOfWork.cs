using Microsoft.EntityFrameworkCore.Storage;
using SportsTeamManagement.Application.Common.Interfaces;
using SportsTeamManagement.Domain.Entities;
using SportsTeamManagement.Infrastructure.Data;

namespace SportsTeamManagement.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Users = new UserRepository(_context);
            Students = new StudentRepository(_context);
            Attendances = new AttendanceRepository(_context);
            Trainers = new Repository<Trainer>(_context);
            TrainingSessions = new Repository<TrainingSession>(_context);
            BeltPromotions = new Repository<BeltPromotion>(_context);
            Events = new Repository<Event>(_context);
            EventRegistrations = new Repository<EventRegistration>(_context);
            Clubs = new Repository<Club>(_context);
            ClubMemberships = new Repository<ClubMembership>(_context);
            TrainingGroups = new Repository<TrainingGroup>(_context);
            Guardians = new Repository<Guardian>(_context);
            MembershipPlans = new Repository<MembershipPlan>(_context);
            Memberships = new Repository<Membership>(_context);
        }

        public IUserRepository Users { get; }
        public IStudentRepository Students { get; }
        public IAttendanceRepository Attendances { get; }
        public IRepository<Trainer> Trainers { get; }
        public IRepository<TrainingSession> TrainingSessions { get; }
        public IRepository<BeltPromotion> BeltPromotions { get; }
        public IRepository<Event> Events { get; }
        public IRepository<EventRegistration> EventRegistrations { get; }
        public IRepository<Club> Clubs { get; }
        public IRepository<ClubMembership> ClubMemberships { get; }
        public IRepository<TrainingGroup> TrainingGroups { get; }
        public IRepository<Guardian> Guardians { get; }
        public IRepository<MembershipPlan> MembershipPlans { get; }
        public IRepository<Membership> Memberships { get; }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
