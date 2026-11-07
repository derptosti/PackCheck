using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PackCheck.Contracts;
using PackCheck.Messages;
using PackCheck.Services;
using System.Collections.ObjectModel;

namespace PackCheck.ViewModels;

public sealed class TripsViewModel : BaseViewModel, ITripsViewModel,
    IRecipient<TripCreatedMessage>, IRecipient<PackingItemUpdatedMessage>, IRecipient<ApiAddressChangedMessage>
{
    private readonly ITripDataService trips;
    private bool needsReload = true;
    private int loadGeneration;

    public TripsViewModel(ITripDataService trips, INavigationService navigation, IMessenger messenger) : base(messenger)
    {
        this.trips = trips;
        RefreshCommand = new AsyncRelayCommand(() => ExecuteAsync(LoadAsync));
        CreateTripCommand = new AsyncRelayCommand(() => ExecuteAsync(() => navigation.NavigateToAsync("create-trip")));
        SettingsCommand = new AsyncRelayCommand(() => ExecuteAsync(() => navigation.NavigateToAsync("settings")));
        OpenTripCommand = new AsyncRelayCommand<TripDto>(trip => ExecuteAsync(async () =>
        {
            if (trip is not null) await navigation.NavigateToAsync("trip", trip.Id);
        }));
    }

    public ObservableCollection<TripDto> Trips { get; } = [];
    public bool IsEmpty => Trips.Count == 0;
    public IAsyncRelayCommand RefreshCommand { get; }
    public IAsyncRelayCommand CreateTripCommand { get; }
    public IAsyncRelayCommand SettingsCommand { get; }
    public IAsyncRelayCommand<TripDto> OpenTripCommand { get; }

    protected override Task OnAppearingAsync() => needsReload ? LoadAsync() : Task.CompletedTask;

    private async Task LoadAsync()
    {
        var generation = loadGeneration;
        var result = await trips.GetAllAsync();
        if (generation != loadGeneration) return;
        Trips.Clear();
        foreach (var trip in result.OrderBy(trip => trip.DepartureDate)) Trips.Add(trip);
        OnPropertyChanged(nameof(IsEmpty));
        needsReload = false;
    }

    private void Invalidate()
    {
        needsReload = true;
        loadGeneration++;
    }

    public void Receive(TripCreatedMessage message) => Invalidate();
    public void Receive(PackingItemUpdatedMessage message) => Invalidate();
    public void Receive(ApiAddressChangedMessage message)
    {
        Invalidate();
        Trips.Clear();
        OnPropertyChanged(nameof(IsEmpty));
    }
}
