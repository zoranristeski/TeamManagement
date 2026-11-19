using AutoMapper;
using SportsTeamManagement.Application.Common.Interfaces;
using SportsTeamManagement.Application.Common.Models;
using SportsTeamManagement.Application.DTOs;
using SportsTeamManagement.Application.Services.Interfaces;
using SportsTeamManagement.Domain.Entities;

namespace SportsTeamManagement.Application.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AttendanceService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<AttendanceDto>> MarkAttendanceAsync(CreateAttendanceDto createAttendanceDto, CancellationToken cancellationToken = default)
        {
            // Check if attendance already exists
            var existingAttendance = await _unitOfWork.Attendances.GetAttendanceByStudentAndSessionAsync(
                createAttendanceDto.StudentId, createAttendanceDto.TrainingSessionId, cancellationToken);

            if (existingAttendance != null)
                return Result.Failure<AttendanceDto>("Attendance already marked for this student and session");

            // Verify student exists
            var student = await _unitOfWork.Students.GetByIdAsync(createAttendanceDto.StudentId, cancellationToken);
            if (student == null)
                return Result.Failure<AttendanceDto>("Student not found");

            // Verify training session exists
            var trainingSession = await _unitOfWork.TrainingSessions.GetByIdAsync(createAttendanceDto.TrainingSessionId, cancellationToken);
            if (trainingSession == null)
                return Result.Failure<AttendanceDto>("Training session not found");

            var attendance = _mapper.Map<Attendance>(createAttendanceDto);
            await _unitOfWork.Attendances.AddAsync(attendance, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var attendanceDto = _mapper.Map<AttendanceDto>(attendance);
            return Result.Success(attendanceDto);
        }

        public async Task<Result<AttendanceDto>> UpdateAttendanceAsync(Guid attendanceId, UpdateAttendanceDto updateAttendanceDto, CancellationToken cancellationToken = default)
        {
            var attendance = await _unitOfWork.Attendances.GetByIdAsync(attendanceId, cancellationToken);
            if (attendance == null)
                return Result.Failure<AttendanceDto>("Attendance record not found");

            _mapper.Map(updateAttendanceDto, attendance);
            await _unitOfWork.Attendances.UpdateAsync(attendance, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var attendanceDto = _mapper.Map<AttendanceDto>(attendance);
            return Result.Success(attendanceDto);
        }

        public async Task<Result<IEnumerable<AttendanceDto>>> GetAttendancesByStudentAsync(Guid studentId, CancellationToken cancellationToken = default)
        {
            var attendances = await _unitOfWork.Attendances.GetAttendancesByStudentAsync(studentId, cancellationToken);
            var attendanceDtos = _mapper.Map<IEnumerable<AttendanceDto>>(attendances);
            return Result.Success(attendanceDtos);
        }

        public async Task<Result<IEnumerable<AttendanceDto>>> GetAttendancesByTrainingSessionAsync(Guid trainingSessionId, CancellationToken cancellationToken = default)
        {
            var attendances = await _unitOfWork.Attendances.GetAttendancesByTrainingSessionAsync(trainingSessionId, cancellationToken);
            var attendanceDtos = _mapper.Map<IEnumerable<AttendanceDto>>(attendances);
            return Result.Success(attendanceDtos);
        }

        public async Task<Result<IEnumerable<AttendanceDto>>> GetAttendancesByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            var attendances = await _unitOfWork.Attendances.GetAttendancesByDateRangeAsync(startDate, endDate, cancellationToken);
            var attendanceDtos = _mapper.Map<IEnumerable<AttendanceDto>>(attendances);
            return Result.Success(attendanceDtos);
        }

        public async Task<Result<AttendanceStatsDto>> GetAttendanceStatsAsync(Guid studentId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(studentId, cancellationToken);
            if (student == null)
                return Result.Failure<AttendanceStatsDto>("Student not found");

            var attendances = await _unitOfWork.Attendances.GetAttendancesByStudentAsync(studentId, cancellationToken);
            var attendanceList = attendances.ToList();

            // Filter by date range if provided
            if (startDate.HasValue)
                attendanceList = attendanceList.Where(a => a.AttendanceDate >= startDate.Value).ToList();
            
            if (endDate.HasValue)
                attendanceList = attendanceList.Where(a => a.AttendanceDate <= endDate.Value).ToList();

            var totalSessions = attendanceList.Count;
            var attendedSessions = attendanceList.Count(a => a.IsPresent);
            var attendancePercentage = totalSessions > 0 ? (double)attendedSessions / totalSessions * 100 : 0;

            var stats = new AttendanceStatsDto
            {
                StudentId = studentId,
                StudentName = $"{student.User?.FirstName} {student.User?.LastName}",
                TotalSessions = totalSessions,
                AttendedSessions = attendedSessions,
                AttendancePercentage = Math.Round(attendancePercentage, 2),
                CurrentStreak = CalculateCurrentStreak(attendanceList),
                LongestStreak = CalculateLongestStreak(attendanceList),
                LastAttendance = attendanceList.Where(a => a.IsPresent).OrderByDescending(a => a.AttendanceDate).FirstOrDefault()?.AttendanceDate
            };

            return Result.Success(stats);
        }

        public async Task<Result<IEnumerable<AttendanceStatsDto>>> GetAllStudentsAttendanceStatsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
        {
            var students = await _unitOfWork.Students.GetAllAsync(cancellationToken);
            var statsList = new List<AttendanceStatsDto>();

            foreach (var student in students)
            {
                var statsResult = await GetAttendanceStatsAsync(student.Id, startDate, endDate, cancellationToken);
                if (statsResult.IsSuccess && statsResult.Value != null)
                {
                    statsList.Add(statsResult.Value);
                }
            }

            return Result.Success<IEnumerable<AttendanceStatsDto>>(statsList);
        }

        public async Task<Result<IEnumerable<AttendanceDto>>> BulkMarkAttendanceAsync(BulkAttendanceDto dto, CancellationToken cancellationToken = default)
        {
            var session = await _unitOfWork.TrainingSessions.GetByIdAsync(dto.TrainingSessionId, cancellationToken);
            if (session is null)
                return Result.Failure<IEnumerable<AttendanceDto>>("Training session not found");
            if (dto.Students.Count == 0)
                return Result.Failure<IEnumerable<AttendanceDto>>("At least one student is required");

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                var created = new List<Attendance>();
                foreach (var item in dto.Students)
                {
                    var student = await _unitOfWork.Students.GetByIdAsync(item.StudentId, cancellationToken);
                    if (student is null)
                        throw new InvalidOperationException($"Student {item.StudentId} not found");

                    var existing = await _unitOfWork.Attendances.GetAttendanceByStudentAndSessionAsync(
                        item.StudentId, dto.TrainingSessionId, cancellationToken);
                    if (existing is not null)
                        continue;

                    created.Add(new Attendance
                    {
                        StudentId = item.StudentId,
                        TrainingSessionId = dto.TrainingSessionId,
                        AttendanceDate = dto.AttendanceDate,
                        IsPresent = item.IsPresent,
                        Notes = item.Notes
                    });
                }

                await _unitOfWork.Attendances.AddRangeAsync(created, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                return Result.Success(_mapper.Map<IEnumerable<AttendanceDto>>(created));
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        private static int CalculateCurrentStreak(List<Attendance> attendances)
        {
            var orderedAttendances = attendances.OrderByDescending(a => a.AttendanceDate).ToList();
            var streak = 0;

            foreach (var attendance in orderedAttendances)
            {
                if (attendance.IsPresent)
                    streak++;
                else
                    break;
            }

            return streak;
        }

        private static int CalculateLongestStreak(List<Attendance> attendances)
        {
            var orderedAttendances = attendances.OrderBy(a => a.AttendanceDate).ToList();
            var longestStreak = 0;
            var currentStreak = 0;

            foreach (var attendance in orderedAttendances)
            {
                if (attendance.IsPresent)
                {
                    currentStreak++;
                    longestStreak = Math.Max(longestStreak, currentStreak);
                }
                else
                {
                    currentStreak = 0;
                }
            }

            return longestStreak;
        }
    }
}
