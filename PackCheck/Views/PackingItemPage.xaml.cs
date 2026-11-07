using PackCheck.ViewModels;

namespace PackCheck.Views;

public partial class PackingItemPage : ContentPage
{
    public PackingItemPage(IPackingItemViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
