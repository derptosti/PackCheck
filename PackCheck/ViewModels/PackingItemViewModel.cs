using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PackCheck.Contracts;
using PackCheck.Messages;
using PackCheck.Services;
using System.Globalization;

namespace PackCheck.ViewModels;

public sealed class PackingItemViewModel : BaseViewModel, IPackingItemViewModel
{
    private readonly IPackingItemDataService items;
    private int tripId;
    private int? itemId;
    private bool loaded;
    private string name = string.Empty;
    private string requiredQuantityText = "1";
    private string packedQuantityText = "0";
    private bool isEssential = true;
    private ItemCategory? selectedCategory;

    public PackingItemViewModel(IPackingItemDataService items, INavigationService navigation,
        IDialogService dialogs, IMessenger messenger) : base(messenger)
    {
        this.items = items;
        SelectedCategory = Categories[0];
        RetryLoadCommand = new AsyncRelayCommand(() => ExecuteAsync(OnAppearingAsync));
        SaveCommand = new AsyncRelayCommand(() => ExecuteAsync(async () =>
        {
            if (!CanSave)
            {
                ErrorMessage = "Load the existing item before saving. Use Retry loading item to try again.";
                return;
            }
            if (string.IsNullOrWhiteSpace(Name) || Name.Trim().Length > 100)
            {
                ErrorMessage = "Enter an item name between 1 and 100 characters.";
                return;
            }
            if (!int.TryParse(RequiredQuantityText, NumberStyles.None, CultureInfo.InvariantCulture, out var required)
                || required is < 1 or > 999)
            {
                ErrorMessage = "Required quantity must be a whole number from 1 to 999.";
                return;
            }
            if (!int.TryParse(PackedQuantityText, NumberStyles.None, CultureInfo.InvariantCulture, out var packed)
                || packed < 0 || packed > required)
            {
                ErrorMessage = "Packed quantity must be between zero and the required quantity.";
                return;
            }
            var request = new SavePackingItemRequest
            {
                Name = Name.Trim(), RequiredQuantity = required, PackedQuantity = packed,
                CategoryKey = string.IsNullOrEmpty(SelectedCategory?.Key) ? null : SelectedCategory.Key,
                IsEssential = IsEssential
            };
            if (itemId is int id) await items.UpdateAsync(tripId, id, request);
            else await items.CreateAsync(tripId, request);
            Messenger.Send(new PackingItemUpdatedMessage(tripId));
            await navigation.GoBackAsync();
        }));
        DeleteCommand = new AsyncRelayCommand(() => ExecuteAsync(async () =>
        {
            if (itemId is not int id) return;
            if (!loaded)
            {
                ErrorMessage = "Load the existing item before deleting. Use Retry loading item to try again.";
                return;
            }
            if (!await dialogs.ConfirmAsync("Delete item?", $"Remove {Name} from this checklist?")) return;
            await items.DeleteAsync(tripId, id);
            Messenger.Send(new PackingItemUpdatedMessage(tripId));
            await navigation.GoBackAsync();
        }));
    }

    public string PageTitle => CanDelete ? "Edit packing item" : "Add packing item";
    public bool CanDelete => itemId.HasValue;
    public bool CanSave => !itemId.HasValue || loaded;
    public bool NeedsItemLoad => itemId.HasValue && !loaded;
    public string Name { get => name; set => SetProperty(ref name, value); }
    public string RequiredQuantityText { get => requiredQuantityText; set => SetProperty(ref requiredQuantityText, value); }
    public string PackedQuantityText { get => packedQuantityText; set => SetProperty(ref packedQuantityText, value); }
    public bool IsEssential { get => isEssential; set => SetProperty(ref isEssential, value); }
    public IReadOnlyList<ItemCategory> Categories { get; } = [new("", "Manual item (no scan category)"), .. ItemCategories.All];
    public ItemCategory? SelectedCategory { get => selectedCategory; set => SetProperty(ref selectedCategory, value); }
    public IAsyncRelayCommand SaveCommand { get; }
    public IAsyncRelayCommand DeleteCommand { get; }
    public IAsyncRelayCommand RetryLoadCommand { get; }

    public void Initialize(object? parameter)
    {
        var target = parameter as ItemNavigationParameter ?? throw new ArgumentException("A trip is required.");
        tripId = target.TripId;
        itemId = target.ItemId;
        loaded = false;
        OnPropertyChanged(nameof(PageTitle));
        OnPropertyChanged(nameof(CanDelete));
        OnPropertyChanged(nameof(CanSave));
        OnPropertyChanged(nameof(NeedsItemLoad));
    }

    protected override async Task OnAppearingAsync()
    {
        if (loaded || itemId is not int id) return;
        var item = await items.GetAsync(tripId, id);
        Name = item.Name;
        RequiredQuantityText = item.RequiredQuantity.ToString(CultureInfo.InvariantCulture);
        PackedQuantityText = item.PackedQuantity.ToString(CultureInfo.InvariantCulture);
        IsEssential = item.IsEssential;
        SelectedCategory = Categories.FirstOrDefault(category => category.Key == item.CategoryKey) ?? Categories[0];
        loaded = true;
        OnPropertyChanged(nameof(CanSave));
        OnPropertyChanged(nameof(NeedsItemLoad));
    }
}
