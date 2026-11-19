using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SportsTeamManagement.Mobile.Models;
using SportsTeamManagement.Mobile.Services;

namespace SportsTeamManagement.Mobile.ViewModels;

public sealed partial class AttendanceViewModel : ObservableObject
{
    private readonly ApiClient _api;

    [ObservableProperty] private string trainingSessionId = string.Empty;
    [ObservableProperty] private string studentId = string.Empty;
    [ObservableProperty] private DateTime attendanceDate = DateTime.Today;
    [ObservableProperty] private bool isPresent = true;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string statusMessage = "Select a session and enter the student identifier.";

    public AttendanceViewModel(ApiClient api)
    {
        _api = api;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy || !Guid.TryParse(TrainingSessionId, out var sessionId) || !Guid.TryParse(StudentId, out var studentId))
        {
            StatusMessage = "Enter valid training session and student GUIDs.";
            return;
        }

        IsBusy = true;
        try
        {
            var dto = new BulkAttendanceDto
            {
                TrainingSessionId = sessionId,
                AttendanceDate = AttendanceDate,
                Students = new List<BulkAttendanceItemDto>
                {
                    new() { StudentId = studentId, IsPresent = IsPresent }
                }
            };
            await _api.PostAsync<BulkAttendanceDto, object>("/api/attendance/bulk", dto);
            StatusMessage = "Attendance saved.";
        }
        catch (Exception)
        {
            StatusMessage = "Unable to save attendance. Verify the session and API connection.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
