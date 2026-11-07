using Microsoft.Extensions.DependencyInjection;
using PackCheck.Views;

namespace PackCheck.Services;

public class NavigationService(IServiceProvider services) : INavigationService
{
    public async Task NavigateToAsync(string route, object? parameter = null)
    {
        var navigation = Shell.Current.Navigation;
        if (route == "trips")
        {
            await navigation.PopToRootAsync();
            return;
        }

        var page = route switch
        {
            "create-trip" => (Page)services.GetRequiredService<CreateTripPage>(),
            "trip" => services.GetRequiredService<TripDetailPage>(),
            "item" => services.GetRequiredService<PackingItemPage>(),
            "scan" => services.GetRequiredService<ScanPage>(),
            "settings" => services.GetRequiredService<SettingsPage>(),
            _ => throw new ArgumentException($"Unknown page: {route}", nameof(route))
        };

        if (page.BindingContext is IInitializableViewModel viewModel)
            viewModel.Initialize(parameter);
        await navigation.PushAsync(page);
    }

    public async Task GoBackAsync() => await Shell.Current.Navigation.PopAsync();
}
