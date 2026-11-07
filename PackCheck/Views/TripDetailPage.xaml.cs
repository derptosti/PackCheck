using PackCheck.ViewModels;

namespace PackCheck.Views;

public partial class TripDetailPage : ContentPage
{
    public TripDetailPage(ITripDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
