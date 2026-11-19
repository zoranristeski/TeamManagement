using SportsTeamManagement.Application.Common.Models;
using SportsTeamManagement.Application.DTOs;

namespace SportsTeamManagement.Application.Services.Interfaces;

public interface ITrainingGroupService
{
    Task<Result<IEnumerable<TrainingGroupDto>>> GetByClubAsync(Guid clubId, CancellationToken cancellationToken = default);
    Task<Result<TrainingGroupDto>> CreateAsync(CreateTrainingGroupDto dto, CancellationToken cancellationToken = default);
}
