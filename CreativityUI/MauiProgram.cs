using Microsoft.Extensions.Logging;
using CreativityUI.Services.Api;

namespace CreativityUI;

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

		builder.Services.AddSingleton(_ => new HttpClient
		{
			BaseAddress = new Uri("https://localhost:7280/")
		});
		builder.Services.AddSingleton<CreativityApiClient>();

		return builder.Build();
	}
}
