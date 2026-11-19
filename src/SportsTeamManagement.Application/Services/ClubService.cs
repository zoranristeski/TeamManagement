using AutoMapper;
using SportsTeamManagement.Application.Common.Interfaces;
using SportsTeamManagement.Application.Common.Models;
using SportsTeamManagement.Application.DTOs;
using SportsTeamManagement.Application.Services.Interfaces;
using SportsTeamManagement.Domain.Entities;

namespace SportsTeamManagement.Application.Services;

public sealed class ClubService(IUnitOfWork unitOfWork, IMapper mapper) : IClubService
{
    public async Task<Result<IEnumerable<ClubDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var clubs = await unitOfWork.Clubs.GetAllAsync(cancellationToken);
        return Result.Success(mapper.Map<IEnumerable<ClubDto>>(clubs));
    }

    public async Task<Result<ClubDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var club = await unitOfWork.Clubs.GetByIdAsync(id, cancellationToken);
        return club is null
            ? Result.Failure<ClubDto>("Club not found")
            : Result.Success(mapper.Map<ClubDto>(club));
    }

    public async Task<Result<ClubDto>> CreateAsync(CreateClubDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Slug))
            return Result.Failure<ClubDto>("Club name and slug are required");

        var club = mapper.Map<Club>(dto);
        await unitOfWork.Clubs.AddAsync(club, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(mapper.Map<ClubDto>(club));
    }

    public async Task<Result<ClubDto>> UpdateAsync(Guid id, UpdateClubDto dto, CancellationToken cancellationToken = default)
    {
        var club = await unitOfWork.Clubs.GetByIdAsync(id, cancellationToken);
        if (club is null)
            return Result.Failure<ClubDto>("Club not found");

        mapper.Map(dto, club);
        await unitOfWork.Clubs.UpdateAsync(club, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(mapper.Map<ClubDto>(club));
    }
}
