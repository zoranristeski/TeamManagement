using SportsTeamManagement.Application.Common.Models;
using SportsTeamManagement.Application.DTOs;

namespace SportsTeamManagement.Application.Services.Interfaces;

public interface IClubService
{
    Task<Result<IEnumerable<ClubDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<ClubDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ClubDto>> CreateAsync(CreateClubDto dto, CancellationToken cancellationToken = default);
    Task<Result<ClubDto>> UpdateAsync(Guid id, UpdateClubDto dto, CancellationToken cancellationToken = default);
}
