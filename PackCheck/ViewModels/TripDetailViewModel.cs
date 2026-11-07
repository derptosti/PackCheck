using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PackCheck.Contracts;
using PackCheck.Messages;
using PackCheck.Services;
using System.Collections.ObjectModel;

namespace PackCheck.ViewModels;

public sealed class TripDetailViewModel : BaseViewModel, ITripDetailViewModel, IRecipient<PackingItemUpdatedMessage>
{
    private readonly ITripDataService trips;
    private readonly IPackingItemDataService items;
    private int tripId;
    private bool needsReload = true;
    private int loadGeneration;
    private TripDto? trip;

    public TripDetailViewModel(ITripDataService trips, IPackingItemDataService items,
        INavigationService navigation, IDialogService dialogs, IMessenger messenger) : base(messenger)
    {
        this.trips = trips;
        this.items = items;
        RefreshCommand = new AsyncRelayCommand(() => ExecuteAsync(LoadAsync));
        AddItemCommand = new AsyncRelayCommand(() => ExecuteAsync(() => navigation.NavigateToAsync("item", new ItemNavigationParameter(tripId, null))));
        ScanCommand = new AsyncRelayCommand(() => ExecuteAsync(() => navigation.NavigateToAsync("scan", tripId)));
        EditItemCommand = new AsyncRelayCommand<PackingItemDto>(item => ExecuteAsync(async () =>
        {
            if (item is not null) await navigation.NavigateToAsync("item", new ItemNavigationParameter(tripId, item.Id));
        }));
        PackOneCommand = new AsyncRelayCommand<PackingItemDto>(item => ExecuteAsync(() => ChangeQuantityAsync(item, 1)));
        UnpackOneCommand = new AsyncRelayCommand<PackingItemDto>(item => ExecuteAsync(() => ChangeQuantityAsync(item, -1)));
        DeleteTripCommand = new AsyncRelayCommand(() => ExecuteAsync(async () =>
        {
            if (!await dialogs.ConfirmAsync("Delete trip?", "This removes the trip and its packing checklist.")) return;
            await trips.DeleteAsync(tripId);
            Messenger.Send(new PackingItemUpdatedMessage(tripId));
            await navigation.GoBackAsync();
        }));
    }

    public TripDto? Trip { get => trip; private set => SetProperty(ref trip, value); }
    public ObservableCollection<PackingItemDto> Items { get; } = [];
    public bool IsEmpty => Items.Count == 0;
    public IAsyncRelayCommand RefreshCommand { get; }
    public IAsyncRelayCommand AddItemCommand { get; }
    public IAsyncRelayCommand ScanCommand { get; }
    public IAsyncRelayCommand DeleteTripCommand { get; }
    public IAsyncRelayCommand<PackingItemDto> EditItemCommand { get; }
    public IAsyncRelayCommand<PackingItemDto> PackOneCommand { get; }
    public IAsyncRelayCommand<PackingItemDto> UnpackOneCommand { get; }

    public void Initialize(object? parameter)
    {
        tripId = parameter is int id ? id : throw new ArgumentException("A trip is required.");
        needsReload = true;
    }

    protected override Task OnAppearingAsync() => needsReload ? LoadAsync() : Task.CompletedTask;

    private async Task LoadAsync()
    {
        var generation = loadGeneration;
        var result = await trips.GetAsync(tripId);
        if (generation != loadGeneration) return;
        Trip = result;
        Items.Clear();
        foreach (var item in Trip.Items.OrderBy(item => item.IsPacked).ThenByDescending(item => item.IsEssential).ThenBy(item => item.Name)) Items.Add(item);
        OnPropertyChanged(nameof(IsEmpty));
        needsReload = false;
    }

    private async Task ChangeQuantityAsync(PackingItemDto? selected, int change)
    {
        if (selected is null) return;
        var current = await items.GetAsync(tripId, selected.Id);
        var quantity = Math.Clamp(current.PackedQuantity + change, 0, current.RequiredQuantity);
        if (quantity == current.PackedQuantity) return;
        await items.UpdateAsync(tripId, current.Id, new SavePackingItemRequest
        {
            Name = current.Name, CategoryKey = current.CategoryKey, RequiredQuantity = current.RequiredQuantity,
            PackedQuantity = quantity, IsEssential = current.IsEssential
        });
        Messenger.Send(new PackingItemUpdatedMessage(tripId));
        await LoadAsync();
    }

    public void Receive(PackingItemUpdatedMessage message)
    {
        if (message.Value == tripId)
        {
            needsReload = true;
            loadGeneration++;
        }
    }
}
