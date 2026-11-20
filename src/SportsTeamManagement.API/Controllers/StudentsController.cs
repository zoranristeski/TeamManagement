using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsTeamManagement.Application.DTOs;
using SportsTeamManagement.Application.Services.Interfaces;
using SportsTeamManagement.Domain.Enums;

namespace SportsTeamManagement.API.Controllers
{
    public class StudentsController : BaseApiController
    {
        private readonly IStudentService _studentService;

        public StudentsController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStudentById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _studentService.GetStudentByIdAsync(id, cancellationToken);
            return HandleResult(result);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetStudentByUserId(Guid userId, CancellationToken cancellationToken)
        {
            var result = await _studentService.GetStudentByUserIdAsync(userId, cancellationToken);
            return HandleResult(result);
        }

        [HttpPost("register")]
        [Authorize]
        public async Task<IActionResult> RegisterStudent([FromBody] StudentRegistrationDto registrationDto, CancellationToken cancellationToken)
        {
            var subject = User.FindFirstValue("sub");
            if (string.IsNullOrWhiteSpace(subject))
            {
                return Unauthorized();
            }

            registrationDto.Auth0Id = subject;
            var result = await _studentService.RegisterStudentAsync(registrationDto, cancellationToken);
            return HandleResult(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStudent(Guid id, [FromBody] UpdateStudentDto updateStudentDto, CancellationToken cancellationToken)
        {
            var result = await _studentService.UpdateStudentAsync(id, updateStudentDto, cancellationToken);
            return HandleResult(result);
        }

        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApproveStudent(Guid id, [FromBody] ApproveStudentRequest request, CancellationToken cancellationToken)
        {
            var result = await _studentService.ApproveStudentAsync(id, request.ApprovedById, cancellationToken);
            return HandleResult(result);
        }

        [HttpPost("{id}/reject")]
        public async Task<IActionResult> RejectStudent(Guid id, [FromBody] RejectStudentRequest request, CancellationToken cancellationToken)
        {
            var result = await _studentService.RejectStudentAsync(id, request.Reason, cancellationToken);
            return HandleResult(result);
        }

        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetStudentsByStatus(StudentStatus status, CancellationToken cancellationToken)
        {
            var result = await _studentService.GetStudentsByStatusAsync(status, cancellationToken);
            return HandleResult(result);
        }

        [HttpGet("belt/{beltRank}")]
        public async Task<IActionResult> GetStudentsByBeltRank(BeltRank beltRank, CancellationToken cancellationToken)
        {
            var result = await _studentService.GetStudentsByBeltRankAsync(beltRank, cancellationToken);
            return HandleResult(result);
        }

        [HttpGet("eligible-for-grading")]
        [Authorize(Policy = "ClubTrainer")]
        public async Task<IActionResult> GetStudentsEligibleForGrading(CancellationToken cancellationToken)
        {
            var result = await _studentService.GetStudentsEligibleForGradingAsync(cancellationToken);
            return HandleResult(result);
        }

        [HttpGet("{id}/attendance-stats")]
        public async Task<IActionResult> GetStudentAttendanceStats(Guid id, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, CancellationToken cancellationToken)
        {
            var result = await _studentService.GetStudentAttendanceStatsAsync(id, startDate, endDate, cancellationToken);
            return HandleResult(result);
        }
    }

    public class ApproveStudentRequest
    {
        public Guid ApprovedById { get; set; }
    }

    public class RejectStudentRequest
    {
        public string Reason { get; set; } = string.Empty;
    }
}
