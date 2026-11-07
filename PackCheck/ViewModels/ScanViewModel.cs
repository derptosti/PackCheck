using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PackCheck.Contracts;
using PackCheck.Messages;
using PackCheck.Services;
using System.Collections.ObjectModel;

namespace PackCheck.ViewModels;

public sealed class ScanViewModel : BaseViewModel, IScanViewModel
{
    private readonly IPackingItemDataService items;
    private readonly IRecognitionService recognition;
    private readonly IPhotoService photos;
    private int tripId;
    private List<PackingItemDto> checklist = [];
    private ImageSource? photoPreview;
    private bool hasResult;
    private bool confirmedForPhoto;
    private string predictionSummary = string.Empty;
    private string confidenceMessage = string.Empty;
    private string confirmationMessage = string.Empty;
    private ItemCategory? selectedCategory;
    private PackingItemDto? selectedItem;

    public ScanViewModel(IPackingItemDataService items, IRecognitionService recognition, IPhotoService photos,
        INavigationService navigation, IMessenger messenger) : base(messenger)
    {
        this.items = items;
        this.recognition = recognition;
        this.photos = photos;
        CaptureCommand = new AsyncRelayCommand(() => ExecuteAsync(() => ChooseAndRecognizeAsync(true)));
        PickCommand = new AsyncRelayCommand(() => ExecuteAsync(() => ChooseAndRecognizeAsync(false)));
        AddItemCommand = new AsyncRelayCommand(() => ExecuteAsync(() => navigation.NavigateToAsync("item", new ItemNavigationParameter(tripId, null))));
        ConfirmCommand = new AsyncRelayCommand(() => ExecuteAsync(ConfirmAsync), CanConfirm);
    }

    public ImageSource? PhotoPreview
    {
        get => photoPreview;
        private set { if (SetProperty(ref photoPreview, value)) OnPropertyChanged(nameof(HasPhoto)); }
    }
    public bool HasPhoto => PhotoPreview is not null;
    public bool HasResult { get => hasResult; private set => SetProperty(ref hasResult, value); }
    public bool HasMatches => MatchingItems.Count > 0;
    public string PredictionSummary { get => predictionSummary; private set => SetProperty(ref predictionSummary, value); }
    public string ConfidenceMessage { get => confidenceMessage; private set => SetProperty(ref confidenceMessage, value); }
    public string ConfirmationMessage { get => confirmationMessage; private set => SetProperty(ref confirmationMessage, value); }
    public IReadOnlyList<ItemCategory> Categories => ItemCategories.All;
    public ObservableCollection<PredictionDto> Predictions { get; } = [];
    public ObservableCollection<PackingItemDto> MatchingItems { get; } = [];
    public ItemCategory? SelectedCategory
    {
        get => selectedCategory;
        set { if (SetProperty(ref selectedCategory, value)) RefreshMatches(); }
    }
    public PackingItemDto? SelectedItem
    {
        get => selectedItem;
        set { if (SetProperty(ref selectedItem, value)) ConfirmCommand?.NotifyCanExecuteChanged(); }
    }
    public IAsyncRelayCommand CaptureCommand { get; }
    public IAsyncRelayCommand PickCommand { get; }
    public IAsyncRelayCommand ConfirmCommand { get; }
    public IAsyncRelayCommand AddItemCommand { get; }

    public void Initialize(object? parameter) => tripId = parameter is int id ? id : throw new ArgumentException("A trip is required.");

    protected override async Task OnAppearingAsync()
    {
        checklist = (await items.GetAllAsync(tripId)).ToList();
        RefreshMatches();
    }

    private async Task ChooseAndRecognizeAsync(bool useCamera)
    {
        var photo = useCamera ? await photos.CaptureAsync() : await photos.PickAsync();
        if (photo is null) return;
        HasResult = false;
        confirmedForPhoto = false;
        ConfirmationMessage = string.Empty;
        SelectedCategory = null;
        Predictions.Clear();
        ConfirmCommand.NotifyCanExecuteChanged();
        PhotoPreview = ImageSource.FromStream(() => new MemoryStream(photo.Bytes, writable: false));
        var result = await recognition.ClassifyImageAsync(photo);
        checklist = (await items.GetAllAsync(tripId)).ToList();
        foreach (var prediction in result.Predictions.OrderByDescending(prediction => prediction.Probability)) Predictions.Add(prediction);
        var best = Predictions.FirstOrDefault();
        PredictionSummary = best is null ? "No prediction returned" : $"Suggested: {best.Label}";
        if (best is null || !best.IsSupported)
            ConfidenceMessage = "The model did not recognize a supported packing item. Choose a category only if it matches your photo.";
        else if (best.Probability < result.ConfidenceThreshold)
            ConfidenceMessage = $"Low confidence (below {result.ConfidenceThreshold:P0}). Check the photo and choose the correct category.";
        else
        {
            ConfidenceMessage = "Check the suggestion, then select the checklist entry you want to pack.";
            SelectedCategory = Categories.FirstOrDefault(category => category.Key == best.CategoryKey);
        }
        HasResult = true;
        ConfirmCommand.NotifyCanExecuteChanged();
    }

    private void RefreshMatches()
    {
        var selectedId = SelectedItem?.Id;
        MatchingItems.Clear();
        if (SelectedCategory is not null)
            foreach (var item in checklist.Where(item => item.CategoryKey == SelectedCategory.Key).OrderBy(item => item.IsPacked).ThenBy(item => item.Name)) MatchingItems.Add(item);
        SelectedItem = MatchingItems.FirstOrDefault(item => item.Id == selectedId)
            ?? MatchingItems.FirstOrDefault(item => !item.IsPacked);
        OnPropertyChanged(nameof(HasMatches));
        ConfirmCommand?.NotifyCanExecuteChanged();
    }

    private bool CanConfirm() => HasResult && !confirmedForPhoto && SelectedCategory is not null
        && SelectedItem is { IsPacked: false } && SelectedItem.CategoryKey == SelectedCategory.Key;

    private async Task ConfirmAsync()
    {
        if (!CanConfirm() || SelectedItem is null) return;
        var current = await items.GetAsync(tripId, SelectedItem.Id);
        if (current.IsPacked || current.CategoryKey != SelectedCategory?.Key)
        {
            await OnAppearingAsync();
            ErrorMessage = "This entry changed. Choose an item that still needs packing.";
            return;
        }
        await items.UpdateAsync(tripId, current.Id, new SavePackingItemRequest
        {
            Name = current.Name, CategoryKey = current.CategoryKey, RequiredQuantity = current.RequiredQuantity,
            PackedQuantity = current.PackedQuantity + 1, IsEssential = current.IsEssential
        });
        confirmedForPhoto = true;
        ConfirmCommand.NotifyCanExecuteChanged();
        ConfirmationMessage = $"Packed one {current.Name}. Take or choose another photo to scan the next item.";
        Messenger.Send(new PackingItemUpdatedMessage(tripId));
        await OnAppearingAsync();
    }
}
