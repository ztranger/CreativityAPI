using CreativityUI.Features.Auth.Pages;
using CreativityUI.Features.Messenger.Pages;
using Microsoft.Extensions.DependencyInjection;

namespace CreativityUI.Features.Auth.Services;

public sealed class AuthNavigationService : IAuthNavigationService
{
    private readonly IServiceProvider _serviceProvider;

    public AuthNavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task OpenRegisterAsync()
    {
        var registerPage = _serviceProvider.GetRequiredService<RegisterPage>();
        var rootPage = Application.Current?.Windows.FirstOrDefault()?.Page;

        if (rootPage is NavigationPage navigationPage)
        {
            await navigationPage.Navigation.PushAsync(registerPage);
            return;
        }

        if (rootPage is not null)
        {
            await rootPage.Navigation.PushAsync(registerPage);
        }
    }

    public Task NavigateToMessengerAsync()
    {
        var messengerPage = _serviceProvider.GetRequiredService<MessengerPage>();
        var window = Application.Current?.Windows.FirstOrDefault();
        if (window is not null)
        {
            window.Page = new NavigationPage(messengerPage);
        }

        return Task.CompletedTask;
    }
}
