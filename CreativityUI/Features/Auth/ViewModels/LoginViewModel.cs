using CreativityUI.Features.Auth.Services;

namespace CreativityUI.Features.Auth.ViewModels;

public sealed class LoginViewModel : ViewModelBase
{
    private readonly IAuthNavigationService _authNavigationService;
    private readonly IAuthTokenStore _authTokenStore;
    private readonly ITokenValidationService _tokenValidationService;
    private string _phone = string.Empty;
    private string _password = string.Empty;
    private string _statusMessage = string.Empty;
    private bool _isTokenValid;
    private string _tokenIndicatorColor = "Red";
    private string _tokenStatusText = "Token отсутствует или невалиден";
    private string _tokenDebugText = "Token: <empty>";
    private string _rawToken = string.Empty;

    public LoginViewModel(
        IAuthNavigationService authNavigationService,
        IAuthTokenStore authTokenStore,
        ITokenValidationService tokenValidationService)
    {
        _authNavigationService = authNavigationService;
        _authTokenStore = authTokenStore;
        _tokenValidationService = tokenValidationService;
        LoginCommand = new Command(OnLogin);
        OpenRegisterCommand = new Command(async () => await OpenRegisterAsync());
        CopyTokenCommand = new Command(async () => await CopyTokenAsync());
        ClearTokenCommand = new Command(async () => await ClearTokenAsync());
        _ = RefreshAsync();
    }

    public string Phone
    {
        get => _phone;
        set => SetProperty(ref _phone, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool IsTokenValid
    {
        get => _isTokenValid;
        set => SetProperty(ref _isTokenValid, value);
    }

    public string TokenIndicatorColor
    {
        get => _tokenIndicatorColor;
        private set => SetProperty(ref _tokenIndicatorColor, value);
    }

    public string TokenStatusText
    {
        get => _tokenStatusText;
        private set => SetProperty(ref _tokenStatusText, value);
    }

    public string TokenDebugText
    {
        get => _tokenDebugText;
        private set => SetProperty(ref _tokenDebugText, value);
    }

    public Command CopyTokenCommand { get; }
    public Command ClearTokenCommand { get; }

    public Command LoginCommand { get; }
    public Command OpenRegisterCommand { get; }

    private void OnLogin()
    {
        if (string.IsNullOrWhiteSpace(Phone) || string.IsNullOrWhiteSpace(Password))
        {
            StatusMessage = "Введите phone и password.";
            return;
        }

        StatusMessage = "Login не реализован на этом этапе.";
    }

    private async Task OpenRegisterAsync()
    {
        await _authNavigationService.OpenRegisterAsync();
    }

    private async Task CopyTokenAsync()
    {
        if (string.IsNullOrWhiteSpace(_rawToken))
        {
            StatusMessage = "Токен пустой, копировать нечего.";
            return;
        }

        await Clipboard.Default.SetTextAsync(_rawToken);
        StatusMessage = "Токен скопирован в буфер обмена.";
    }

    private async Task ClearTokenAsync()
    {
        await _authTokenStore.ClearTokenAsync();
        UpdateTokenPresentation(null);
        StatusMessage = "Токен очищен.";
    }

    public async Task RefreshAsync()
    {
        var persistedPhone = await _authTokenStore.GetPhoneAsync();
        if (!string.IsNullOrWhiteSpace(persistedPhone))
        {
            Phone = persistedPhone;
        }

        var persistedToken = await _authTokenStore.GetTokenAsync();
        UpdateTokenPresentation(persistedToken);
    }

    private void UpdateTokenPresentation(string? token)
    {
        IsTokenValid = _tokenValidationService.IsTokenValid(token);
        TokenIndicatorColor = IsTokenValid ? "Green" : "Red";
        TokenStatusText = IsTokenValid
            ? "Token валиден"
            : "Token отсутствует или невалиден";
        _rawToken = token ?? string.Empty;
        TokenDebugText = string.IsNullOrWhiteSpace(token)
            ? "Token: <empty>"
            : $"Token: {token}";
    }
}
