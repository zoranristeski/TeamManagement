namespace SportsTeamManagement.Application.DTOs
{
    public class AttendanceDto
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public Guid TrainingSessionId { get; set; }
        public StudentDto Student { get; set; } = null!;
        public TrainingSessionDto TrainingSession { get; set; } = null!;
        public DateTime AttendanceDate { get; set; }
        public bool IsPresent { get; set; }
        public string? Notes { get; set; }
    }

    public class CreateAttendanceDto
    {
        public Guid StudentId { get; set; }
        public Guid TrainingSessionId { get; set; }
        public DateTime AttendanceDate { get; set; }
        public bool IsPresent { get; set; } = true;
        public string? Notes { get; set; }
    }

    public class UpdateAttendanceDto
    {
        public bool IsPresent { get; set; }
        public string? Notes { get; set; }
    }

    public class BulkAttendanceItemDto
    {
        public Guid StudentId { get; set; }
        public bool IsPresent { get; set; } = true;
        public string? Notes { get; set; }
    }

    public class BulkAttendanceDto
    {
        public Guid TrainingSessionId { get; set; }
        public DateTime AttendanceDate { get; set; }
        public List<BulkAttendanceItemDto> Students { get; set; } = new();
    }

    public class AttendanceStatsDto
    {
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int TotalSessions { get; set; }
        public int AttendedSessions { get; set; }
        public double AttendancePercentage { get; set; }
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public DateTime? LastAttendance { get; set; }
    }
}
