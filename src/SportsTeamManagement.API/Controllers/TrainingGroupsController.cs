using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsTeamManagement.Application.DTOs;
using SportsTeamManagement.Application.Services.Interfaces;

namespace SportsTeamManagement.API.Controllers;

[Authorize(Policy = "ClubManager")]
public sealed class TrainingGroupsController(ITrainingGroupService groupService) : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetByClub([FromQuery] Guid clubId, CancellationToken cancellationToken)
    {
        var result = await groupService.GetByClubAsync(clubId, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTrainingGroupDto dto, CancellationToken cancellationToken)
    {
        var result = await groupService.CreateAsync(dto, cancellationToken);
        return HandleResult(result);
    }
}
