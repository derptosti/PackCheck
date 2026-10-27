using PackCheck.ViewModels.Interfaces;

namespace PackCheck.Views;

public partial class TripsPage : ContentPage
{
    public TripsPage(ITripsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}