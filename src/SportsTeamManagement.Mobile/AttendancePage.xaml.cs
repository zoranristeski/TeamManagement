using SportsTeamManagement.Mobile.ViewModels;

namespace SportsTeamManagement.Mobile;

public partial class AttendancePage : ContentPage
{
    public AttendancePage(AttendanceViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
