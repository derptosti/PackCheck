using PackCheck.ViewModels;

namespace PackCheck.Views;

public partial class CreateTripPage : ContentPage
{
    public CreateTripPage(ICreateTripViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
