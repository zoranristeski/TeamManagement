namespace SportsTeamManagement.Mobile;

public partial class AppShell : Shell
{
	public AppShell(MainPage mainPage, AttendancePage attendancePage)
	{
		InitializeComponent();
		Items.Add(new ShellContent
		{
			Title = "Overview",
			Content = mainPage,
			Route = "overview"
		});
		Items.Add(new ShellContent
		{
			Title = "Attendance",
			Content = attendancePage,
			Route = "attendance"
		});
	}
}
