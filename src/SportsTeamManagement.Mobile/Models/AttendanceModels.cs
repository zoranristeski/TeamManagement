namespace SportsTeamManagement.Mobile.Models;

public sealed class AttendanceStatsDto
{
    public Guid StudentId { get; set; }
    public int TotalSessions { get; set; }
    public int AttendedSessions { get; set; }
    public double AttendancePercentage { get; set; }
}

public sealed class BulkAttendanceItemDto
{
    public Guid StudentId { get; set; }
    public bool IsPresent { get; set; } = true;
    public string? Notes { get; set; }
}

public sealed class BulkAttendanceDto
{
    public Guid TrainingSessionId { get; set; }
    public DateTime AttendanceDate { get; set; }
    public List<BulkAttendanceItemDto> Students { get; set; } = new();
}
