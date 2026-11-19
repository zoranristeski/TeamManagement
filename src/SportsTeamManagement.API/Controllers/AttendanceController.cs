using Microsoft.AspNetCore.Mvc;
using SportsTeamManagement.Application.DTOs;
using SportsTeamManagement.Application.Services.Interfaces;

namespace SportsTeamManagement.API.Controllers
{
    public class AttendanceController : BaseApiController
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        [HttpPost]
        public async Task<IActionResult> MarkAttendance([FromBody] CreateAttendanceDto createAttendanceDto, CancellationToken cancellationToken)
        {
            var result = await _attendanceService.MarkAttendanceAsync(createAttendanceDto, cancellationToken);
            return HandleResult(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAttendance(Guid id, [FromBody] UpdateAttendanceDto updateAttendanceDto, CancellationToken cancellationToken)
        {
            var result = await _attendanceService.UpdateAttendanceAsync(id, updateAttendanceDto, cancellationToken);
            return HandleResult(result);
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetAttendancesByStudent(Guid studentId, CancellationToken cancellationToken)
        {
            var result = await _attendanceService.GetAttendancesByStudentAsync(studentId, cancellationToken);
            return HandleResult(result);
        }

        [HttpGet("session/{sessionId}")]
        public async Task<IActionResult> GetAttendancesByTrainingSession(Guid sessionId, CancellationToken cancellationToken)
        {
            var result = await _attendanceService.GetAttendancesByTrainingSessionAsync(sessionId, cancellationToken);
            return HandleResult(result);
        }

        [HttpGet("date-range")]
        public async Task<IActionResult> GetAttendancesByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, CancellationToken cancellationToken)
        {
            var result = await _attendanceService.GetAttendancesByDateRangeAsync(startDate, endDate, cancellationToken);
            return HandleResult(result);
        }

        [HttpGet("stats/{studentId}")]
        public async Task<IActionResult> GetAttendanceStats(Guid studentId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, CancellationToken cancellationToken)
        {
            var result = await _attendanceService.GetAttendanceStatsAsync(studentId, startDate, endDate, cancellationToken);
            return HandleResult(result);
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetAllStudentsAttendanceStats([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, CancellationToken cancellationToken)
        {
            var result = await _attendanceService.GetAllStudentsAttendanceStatsAsync(startDate, endDate, cancellationToken);
            return HandleResult(result);
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> BulkMarkAttendance([FromBody] BulkAttendanceDto dto, CancellationToken cancellationToken)
        {
            var result = await _attendanceService.BulkMarkAttendanceAsync(dto, cancellationToken);
            return HandleResult(result);
        }
    }
}
