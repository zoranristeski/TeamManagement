using AutoMapper;
using SportsTeamManagement.Application.Common.Interfaces;
using SportsTeamManagement.Application.Common.Models;
using SportsTeamManagement.Application.DTOs;
using SportsTeamManagement.Application.Services.Interfaces;
using SportsTeamManagement.Domain.Entities;

namespace SportsTeamManagement.Application.Services;

public sealed class TrainingGroupService(IUnitOfWork unitOfWork, IMapper mapper) : ITrainingGroupService
{
    public async Task<Result<IEnumerable<TrainingGroupDto>>> GetByClubAsync(Guid clubId, CancellationToken cancellationToken = default)
    {
        var groups = await unitOfWork.TrainingGroups.FindAsync(
            x => x.ClubId == clubId && !x.IsDeleted,
            cancellationToken);
        return Result.Success(mapper.Map<IEnumerable<TrainingGroupDto>>(groups));
    }

    public async Task<Result<TrainingGroupDto>> CreateAsync(CreateTrainingGroupDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.ClubId == Guid.Empty)
            return Result.Failure<TrainingGroupDto>("ClubId is required");
        if (string.IsNullOrWhiteSpace(dto.Name))
            return Result.Failure<TrainingGroupDto>("Group name is required");
        if (dto.Capacity < 1)
            return Result.Failure<TrainingGroupDto>("Capacity must be greater than zero");

        var club = await unitOfWork.Clubs.GetByIdAsync(dto.ClubId, cancellationToken);
        if (club is null)
            return Result.Failure<TrainingGroupDto>("Club not found");

        var group = mapper.Map<TrainingGroup>(dto);
        await unitOfWork.TrainingGroups.AddAsync(group, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(mapper.Map<TrainingGroupDto>(group));
    }
}
