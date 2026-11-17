using AutoMapper;
using SportsTeamManagement.Application.Common.Interfaces;
using SportsTeamManagement.Application.Common.Models;
using SportsTeamManagement.Application.DTOs;
using SportsTeamManagement.Application.Services.Interfaces;
using SportsTeamManagement.Domain.Entities;

namespace SportsTeamManagement.Application.Services
{
    public class TrainingSessionService : ITrainingSessionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TrainingSessionService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<TrainingSessionDto>> GetTrainingSessionByIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
        {
            var session = await _unitOfWork.TrainingSessions.GetByIdAsync(sessionId, cancellationToken);
            if (session == null)
                return Result.Failure<TrainingSessionDto>("Training session not found");

            var sessionDto = _mapper.Map<TrainingSessionDto>(session);
            return Result.Success(sessionDto);
        }

        public async Task<Result<TrainingSessionDto>> CreateTrainingSessionAsync(CreateTrainingSessionDto createSessionDto, CancellationToken cancellationToken = default)
        {
            // Verify trainer exists
            var trainer = await _unitOfWork.Trainers.GetByIdAsync(createSessionDto.TrainerId, cancellationToken);
            if (trainer == null)
                return Result.Failure<TrainingSessionDto>("Trainer not found");

            // Validate session times
            if (createSessionDto.StartTime >= createSessionDto.EndTime)
                return Result.Failure<TrainingSessionDto>("Start time must be before end time");

            var session = _mapper.Map<TrainingSession>(createSessionDto);
            await _unitOfWork.TrainingSessions.AddAsync(session, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var sessionDto = _mapper.Map<TrainingSessionDto>(session);
            return Result.Success(sessionDto);
        }

        public async Task<Result<TrainingSessionDto>> UpdateTrainingSessionAsync(Guid sessionId, UpdateTrainingSessionDto updateSessionDto, CancellationToken cancellationToken = default)
        {
            var session = await _unitOfWork.TrainingSessions.GetByIdAsync(sessionId, cancellationToken);
            if (session == null)
                return Result.Failure<TrainingSessionDto>("Training session not found");

            // Validate session times
            if (updateSessionDto.StartTime >= updateSessionDto.EndTime)
                return Result.Failure<TrainingSessionDto>("Start time must be before end time");

            _mapper.Map(updateSessionDto, session);
            await _unitOfWork.TrainingSessions.UpdateAsync(session, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var sessionDto = _mapper.Map<TrainingSessionDto>(session);
            return Result.Success(sessionDto);
        }

        public async Task<Result> DeleteTrainingSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
        {
            var session = await _unitOfWork.TrainingSessions.GetByIdAsync(sessionId, cancellationToken);
            if (session == null)
                return Result.Failure("Training session not found");

            await _unitOfWork.TrainingSessions.DeleteAsync(session, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }

        public async Task<Result<IEnumerable<TrainingSessionDto>>> GetAllTrainingSessionsAsync(CancellationToken cancellationToken = default)
        {
            var sessions = await _unitOfWork.TrainingSessions.GetAllAsync(cancellationToken);
            var sessionDtos = _mapper.Map<IEnumerable<TrainingSessionDto>>(sessions);
            return Result.Success(sessionDtos);
        }

        public async Task<Result<IEnumerable<TrainingSessionDto>>> GetTrainingSessionsByTrainerAsync(Guid trainerId, CancellationToken cancellationToken = default)
        {
            var sessions = await _unitOfWork.TrainingSessions.FindAsync(s => s.TrainerId == trainerId, cancellationToken);
            var sessionDtos = _mapper.Map<IEnumerable<TrainingSessionDto>>(sessions);
            return Result.Success(sessionDtos);
        }

        public async Task<Result<IEnumerable<TrainingSessionDto>>> GetUpcomingTrainingSessionsAsync(CancellationToken cancellationToken = default)
        {
            var currentTime = DateTime.UtcNow;
            var sessions = await _unitOfWork.TrainingSessions.FindAsync(
                s => s.StartTime > currentTime && s.IsActive, cancellationToken);
            
            var sessionDtos = _mapper.Map<IEnumerable<TrainingSessionDto>>(sessions.OrderBy(s => s.StartTime));
            return Result.Success(sessionDtos);
        }
    }
}
