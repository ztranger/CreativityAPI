using Microsoft.Extensions.Logging;
using CreativityUI.Services.Api;
using CreativityUI.Features.Auth.Pages;
using CreativityUI.Features.Auth.Services;
using CreativityUI.Features.Auth.ViewModels;

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
			BaseAddress = new Uri("https://localhost:7575/")
		});
		builder.Services.AddSingleton<CreativityApiClient>();
		builder.Services.AddSingleton<IAuthTokenStore, SecureAuthTokenStore>();
		builder.Services.AddSingleton<IAuthApiClient, AuthApiClient>();
		builder.Services.AddSingleton<IAuthService, AuthService>();
		builder.Services.AddSingleton<ITokenValidationService, JwtTokenValidationService>();
		builder.Services.AddSingleton<IAuthNavigationService, AuthNavigationService>();
		builder.Services.AddTransient<LoginViewModel>();
		builder.Services.AddTransient<RegisterViewModel>();
		builder.Services.AddTransient<LoginPage>();
		builder.Services.AddTransient<RegisterPage>();

		return builder.Build();
	}
}
