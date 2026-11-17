using Microsoft.AspNetCore.Mvc;
using SportsTeamManagement.Application.Common.Models;

namespace SportsTeamManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseApiController : ControllerBase
    {
        protected IActionResult HandleResult<T>(Result<T> result)
        {
            if (result.IsSuccess)
                return Ok(result.Value);

            if (result.Errors.Any())
                return BadRequest(result.Errors);

            return BadRequest(result.ErrorMessage);
        }

        protected IActionResult HandleResult(Result result)
        {
            if (result.IsSuccess)
                return Ok();

            if (result.Errors.Any())
                return BadRequest(result.Errors);

            return BadRequest(result.ErrorMessage);
        }
    }
}
