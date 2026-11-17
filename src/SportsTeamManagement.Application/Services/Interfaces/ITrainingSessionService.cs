using SportsTeamManagement.Application.Common.Models;
using SportsTeamManagement.Application.DTOs;

namespace SportsTeamManagement.Application.Services.Interfaces
{
    public interface ITrainingSessionService
    {
        Task<Result<TrainingSessionDto>> GetTrainingSessionByIdAsync(Guid sessionId, CancellationToken cancellationToken = default);
        Task<Result<TrainingSessionDto>> CreateTrainingSessionAsync(CreateTrainingSessionDto createSessionDto, CancellationToken cancellationToken = default);
        Task<Result<TrainingSessionDto>> UpdateTrainingSessionAsync(Guid sessionId, UpdateTrainingSessionDto updateSessionDto, CancellationToken cancellationToken = default);
        Task<Result> DeleteTrainingSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);
        Task<Result<IEnumerable<TrainingSessionDto>>> GetAllTrainingSessionsAsync(CancellationToken cancellationToken = default);
        Task<Result<IEnumerable<TrainingSessionDto>>> GetTrainingSessionsByTrainerAsync(Guid trainerId, CancellationToken cancellationToken = default);
        Task<Result<IEnumerable<TrainingSessionDto>>> GetUpcomingTrainingSessionsAsync(CancellationToken cancellationToken = default);
    }
}
