using CreativityUI.Features.Messenger.ViewModels;

namespace CreativityUI.Features.Messenger.Pages;

public partial class MessengerPage : ContentPage
{
    private readonly MessengerViewModel _viewModel;
    private bool _isInitialized;

    public MessengerPage(MessengerViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;
        await _viewModel.InitializeAsync();
    }
}
