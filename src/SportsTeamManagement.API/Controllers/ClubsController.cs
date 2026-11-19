using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsTeamManagement.Application.DTOs;
using SportsTeamManagement.Application.Services.Interfaces;

namespace SportsTeamManagement.API.Controllers;

[Authorize(Roles = "PlatformAdmin")]
public sealed class ClubsController(IClubService clubService) : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await clubService.GetAllAsync(cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "PlatformAdmin,ClubAdmin")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await clubService.GetByIdAsync(id, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClubDto dto, CancellationToken cancellationToken)
    {
        var result = await clubService.CreateAsync(dto, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "PlatformAdmin,ClubAdmin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClubDto dto, CancellationToken cancellationToken)
    {
        var result = await clubService.UpdateAsync(id, dto, cancellationToken);
        return HandleResult(result);
    }
}
