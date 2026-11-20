using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SportsTeamManagement.Mobile.Models;
using SportsTeamManagement.Mobile.Services;

namespace SportsTeamManagement.Mobile.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly ApiClient _api;

    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string password = string.Empty;
    [ObservableProperty] private string statusMessage = "Sign in with your TeamHub account.";
    [ObservableProperty] private int activeClubCount;
    [ObservableProperty] private double averageAttendance;

    public MainViewModel(ApiClient api)
    {
        _api = api;
    }

    [RelayCommand]
    private async Task SignInAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password)) return;
        try
        {
            await _api.LoginAsync(Email, Password);
            StatusMessage = "Signed in.";
            await RefreshAsync();
        }
        catch (Exception)
        {
            StatusMessage = "Sign in failed.";
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var clubs = await _api.GetAsync<List<ClubDto>>("/api/clubs");
            var stats = await _api.GetAsync<List<AttendanceStatsDto>>("/api/attendance/stats");
            ActiveClubCount = clubs?.Count ?? 0;
            AverageAttendance = stats is { Count: > 0 }
                ? stats.Average(x => x.AttendancePercentage)
                : 0;
            StatusMessage = "Portal data synchronized.";
        }
        catch (Exception)
        {
            StatusMessage = "Unable to synchronize. Check the API address and access token.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SignOutAsync()
    {
        await _api.SignOutAsync();
        ActiveClubCount = 0;
        AverageAttendance = 0;
        Email = string.Empty;
        Password = string.Empty;
        StatusMessage = "Signed out.";
    }
}
