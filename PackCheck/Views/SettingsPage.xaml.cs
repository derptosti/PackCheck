using PackCheck.ViewModels;

namespace PackCheck.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(ISettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
