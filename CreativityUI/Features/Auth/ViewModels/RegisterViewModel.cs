using API.Contracts.Auth;
using CreativityUI.Features.Auth.Services;

namespace CreativityUI.Features.Auth.ViewModels;

public sealed class RegisterViewModel : ViewModelBase
{
    private readonly IAuthService _authService;
    private readonly IAuthTokenStore _authTokenStore;
    private string _phone = string.Empty;
    private string _displayName = string.Empty;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _resultMessage = string.Empty;
    private bool _isBusy;

    public RegisterViewModel(IAuthService authService, IAuthTokenStore authTokenStore)
    {
        _authService = authService;
        _authTokenStore = authTokenStore;
        SubmitCommand = new Command(async () => await SubmitAsync(), () => !IsBusy);
        _ = InitializeAsync();
    }

    public string Phone
    {
        get => _phone;
        set => SetProperty(ref _phone, value);
    }

    public string DisplayName
    {
        get => _displayName;
        set => SetProperty(ref _displayName, value);
    }

    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public string ResultMessage
    {
        get => _resultMessage;
        set => SetProperty(ref _resultMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                SubmitCommand.ChangeCanExecute();
            }
        }
    }

    public Command SubmitCommand { get; }

    private async Task SubmitAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(Phone) || string.IsNullOrWhiteSpace(DisplayName) || string.IsNullOrWhiteSpace(Password))
        {
            ResultMessage = "Заполните Phone, DisplayName и Password.";
            return;
        }

        IsBusy = true;
        ResultMessage = string.Empty;

        try
        {
            var request = new RegisterRequest
            {
                Phone = Phone.Trim(),
                DisplayName = DisplayName.Trim(),
                Username = string.IsNullOrWhiteSpace(Username) ? null : Username.Trim(),
                Password = Password
            };
            var response = await _authService.RegisterAsync(request);
            await _authTokenStore.SavePhoneAsync(request.Phone);
            ResultMessage = $"Registered: id={response.User.Id}, name={response.User.DisplayName}, token saved.";
        }
        catch (Exception ex)
        {
            ResultMessage = $"Register failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task InitializeAsync()
    {
        var persistedPhone = await _authTokenStore.GetPhoneAsync();
        if (!string.IsNullOrWhiteSpace(persistedPhone))
        {
            Phone = persistedPhone;
        }
    }
}
