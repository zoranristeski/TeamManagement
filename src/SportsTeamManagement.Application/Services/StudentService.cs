using AutoMapper;
using SportsTeamManagement.Application.Common.Interfaces;
using SportsTeamManagement.Application.Common.Models;
using SportsTeamManagement.Application.DTOs;
using SportsTeamManagement.Application.Services.Interfaces;
using SportsTeamManagement.Domain.Entities;
using SportsTeamManagement.Domain.Enums;

namespace SportsTeamManagement.Application.Services
{
    public class StudentService : IStudentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public StudentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<StudentDto>> GetStudentByIdAsync(Guid studentId, CancellationToken cancellationToken = default)
        {
            var student = await _unitOfWork.Students.FirstOrDefaultAsync(
                s => s.Id == studentId, cancellationToken);
            
            if (student == null)
                return Result.Failure<StudentDto>("Student not found");

            var studentDto = _mapper.Map<StudentDto>(student);
            return Result.Success(studentDto);
        }

        public async Task<Result<StudentDto>> GetStudentByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var student = await _unitOfWork.Students.FirstOrDefaultAsync(
                s => s.UserId == userId, cancellationToken);
            
            if (student == null)
                return Result.Failure<StudentDto>("Student not found");

            var studentDto = _mapper.Map<StudentDto>(student);
            return Result.Success(studentDto);
        }

        public async Task<Result<StudentDto>> RegisterStudentAsync(StudentRegistrationDto registrationDto, CancellationToken cancellationToken = default)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                // Check if user already exists
                var existingUser = await _unitOfWork.Users.GetByAuth0IdAsync(registrationDto.Auth0Id, cancellationToken);
                if (existingUser != null)
                    return Result.Failure<StudentDto>("User with this Auth0 ID already exists");

                var existingEmailUser = await _unitOfWork.Users.GetByEmailAsync(registrationDto.Email, cancellationToken);
                if (existingEmailUser != null)
                    return Result.Failure<StudentDto>("User with this email already exists");

                // Create user
                var user = _mapper.Map<User>(registrationDto);
                user.Role = UserRole.Student;
                await _unitOfWork.Users.AddAsync(user, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Create student
                var student = new Student
                {
                    UserId = user.Id,
                    DateOfBirth = registrationDto.DateOfBirth,
                    EmergencyContactName = registrationDto.EmergencyContactName,
                    EmergencyContactPhone = registrationDto.EmergencyContactPhone,
                    MedicalConditions = registrationDto.MedicalConditions,
                    Status = StudentStatus.PendingApproval,
                    CurrentBelt = BeltRank.White
                };

                await _unitOfWork.Students.AddAsync(student, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                // Load student with user data
                var createdStudent = await _unitOfWork.Students.FirstOrDefaultAsync(
                    s => s.Id == student.Id, cancellationToken);
                
                var studentDto = _mapper.Map<StudentDto>(createdStudent);
                return Result.Success(studentDto);
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<StudentDto>("Failed to register student");
            }
        }

        public async Task<Result<StudentDto>> UpdateStudentAsync(Guid studentId, UpdateStudentDto updateStudentDto, CancellationToken cancellationToken = default)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(studentId, cancellationToken);
            if (student == null)
                return Result.Failure<StudentDto>("Student not found");

            _mapper.Map(updateStudentDto, student);
            await _unitOfWork.Students.UpdateAsync(student, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var studentDto = _mapper.Map<StudentDto>(student);
            return Result.Success(studentDto);
        }

        public async Task<Result> ApproveStudentAsync(Guid studentId, Guid approvedById, CancellationToken cancellationToken = default)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(studentId, cancellationToken);
            if (student == null)
                return Result.Failure("Student not found");

            if (student.Status != StudentStatus.PendingApproval)
                return Result.Failure("Student is not pending approval");

            student.Status = StudentStatus.Active;
            await _unitOfWork.Students.UpdateAsync(student, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }

        public async Task<Result> RejectStudentAsync(Guid studentId, string reason, CancellationToken cancellationToken = default)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(studentId, cancellationToken);
            if (student == null)
                return Result.Failure("Student not found");

            if (student.Status != StudentStatus.PendingApproval)
                return Result.Failure("Student is not pending approval");

            student.Status = StudentStatus.Inactive;
            await _unitOfWork.Students.UpdateAsync(student, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }

        public async Task<Result<IEnumerable<StudentDto>>> GetStudentsByStatusAsync(StudentStatus status, CancellationToken cancellationToken = default)
        {
            var students = await _unitOfWork.Students.GetStudentsByStatusAsync(status, cancellationToken);
            var studentDtos = _mapper.Map<IEnumerable<StudentDto>>(students);
            return Result.Success(studentDtos);
        }

        public async Task<Result<IEnumerable<StudentDto>>> GetStudentsByBeltRankAsync(BeltRank beltRank, CancellationToken cancellationToken = default)
        {
            var students = await _unitOfWork.Students.GetStudentsByBeltRankAsync(beltRank, cancellationToken);
            var studentDtos = _mapper.Map<IEnumerable<StudentDto>>(students);
            return Result.Success(studentDtos);
        }

        public async Task<Result<IEnumerable<StudentDto>>> GetStudentsEligibleForGradingAsync(CancellationToken cancellationToken = default)
        {
            var students = await _unitOfWork.Students.GetStudentsEligibleForGradingAsync(cancellationToken);
            var studentDtos = _mapper.Map<IEnumerable<StudentDto>>(students);
            return Result.Success(studentDtos);
        }

        public async Task<Result<AttendanceStatsDto>> GetStudentAttendanceStatsAsync(Guid studentId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(studentId, cancellationToken);
            if (student == null)
                return Result.Failure<AttendanceStatsDto>("Student not found");

            var attendanceCount = await _unitOfWork.Attendances.GetAttendanceCountByStudentAsync(
                studentId, startDate, endDate, cancellationToken);

            var attendances = await _unitOfWork.Attendances.GetAttendancesByStudentAsync(studentId, cancellationToken);
            var attendanceList = attendances.ToList();

            var totalSessions = attendanceList.Count;
            var attendedSessions = attendanceList.Count(a => a.IsPresent);
            var attendancePercentage = totalSessions > 0 ? (double)attendedSessions / totalSessions * 100 : 0;

            // Calculate streaks
            var currentStreak = CalculateCurrentStreak(attendanceList);
            var longestStreak = CalculateLongestStreak(attendanceList);
            var lastAttendance = attendanceList.Where(a => a.IsPresent).OrderByDescending(a => a.AttendanceDate).FirstOrDefault()?.AttendanceDate;

            var stats = new AttendanceStatsDto
            {
                StudentId = studentId,
                StudentName = $"{student.User?.FirstName} {student.User?.LastName}",
                TotalSessions = totalSessions,
                AttendedSessions = attendedSessions,
                AttendancePercentage = Math.Round(attendancePercentage, 2),
                CurrentStreak = currentStreak,
                LongestStreak = longestStreak,
                LastAttendance = lastAttendance
            };

            return Result.Success(stats);
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
