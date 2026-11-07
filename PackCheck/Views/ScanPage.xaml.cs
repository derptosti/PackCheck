using PackCheck.ViewModels;

namespace PackCheck.Views;

public partial class ScanPage : ContentPage
{
    public ScanPage(IScanViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
