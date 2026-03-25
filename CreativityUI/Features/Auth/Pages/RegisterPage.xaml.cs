using CreativityUI.Features.Auth.ViewModels;

namespace CreativityUI.Features.Auth.Pages;

public partial class RegisterPage : ContentPage
{
    public RegisterPage(RegisterViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
