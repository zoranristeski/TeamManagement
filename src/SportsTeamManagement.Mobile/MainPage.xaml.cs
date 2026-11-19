using SportsTeamManagement.Mobile.ViewModels;

namespace SportsTeamManagement.Mobile;

public partial class MainPage : ContentPage
{
	public MainPage(MainViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}

