using Microsoft.Extensions.Logging;
using SportsTeamManagement.Mobile.Services;
using SportsTeamManagement.Mobile.ViewModels;

namespace SportsTeamManagement.Mobile;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		builder.Services.AddSingleton<ApiClient>(serviceProvider =>
		{
			var configuredBaseUrl = Preferences.Default.Get("ApiBaseUrl", "https://localhost:7001");
			var httpClient = new HttpClient
			{
				BaseAddress = new Uri(configuredBaseUrl.TrimEnd('/') + "/")
			};
			return new ApiClient(httpClient);
		});
		builder.Services.AddSingleton<MainViewModel>();
		builder.Services.AddSingleton<MainPage>();
		builder.Services.AddSingleton<AttendanceViewModel>();
		builder.Services.AddSingleton<AttendancePage>();
		builder.Services.AddSingleton<AppShell>();

		return builder.Build();
	}
}
