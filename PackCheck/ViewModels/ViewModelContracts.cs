using CommunityToolkit.Mvvm.Input;
using PackCheck.Contracts;
using PackCheck.Services;
using System.Collections.ObjectModel;

namespace PackCheck.ViewModels;

public interface IViewModel : IPageViewModel
{
    bool IsBusy { get; }
    string ErrorMessage { get; }
    bool HasError { get; }
}

public interface ITripsViewModel : IViewModel
{
    ObservableCollection<TripDto> Trips { get; }
    bool IsEmpty { get; }
    IAsyncRelayCommand RefreshCommand { get; }
    IAsyncRelayCommand CreateTripCommand { get; }
    IAsyncRelayCommand SettingsCommand { get; }
    IAsyncRelayCommand<TripDto> OpenTripCommand { get; }
}

public interface ICreateTripViewModel : IViewModel
{
    string Name { get; set; }
    DateTime DepartureDate { get; set; }
    ObservableCollection<PackingTemplateDto> Templates { get; }
    PackingTemplateDto? SelectedTemplate { get; set; }
    IAsyncRelayCommand SaveCommand { get; }
    IAsyncRelayCommand ReloadCommand { get; }
}

public interface ITripDetailViewModel : IViewModel, IInitializableViewModel
{
    TripDto? Trip { get; }
    ObservableCollection<PackingItemDto> Items { get; }
    bool IsEmpty { get; }
    IAsyncRelayCommand RefreshCommand { get; }
    IAsyncRelayCommand AddItemCommand { get; }
    IAsyncRelayCommand ScanCommand { get; }
    IAsyncRelayCommand DeleteTripCommand { get; }
    IAsyncRelayCommand<PackingItemDto> EditItemCommand { get; }
    IAsyncRelayCommand<PackingItemDto> PackOneCommand { get; }
    IAsyncRelayCommand<PackingItemDto> UnpackOneCommand { get; }
}

public interface IPackingItemViewModel : IViewModel, IInitializableViewModel
{
    string PageTitle { get; }
    string Name { get; set; }
    string RequiredQuantityText { get; set; }
    string PackedQuantityText { get; set; }
    bool IsEssential { get; set; }
    bool CanDelete { get; }
    bool CanSave { get; }
    bool NeedsItemLoad { get; }
    IReadOnlyList<ItemCategory> Categories { get; }
    ItemCategory? SelectedCategory { get; set; }
    IAsyncRelayCommand SaveCommand { get; }
    IAsyncRelayCommand DeleteCommand { get; }
    IAsyncRelayCommand RetryLoadCommand { get; }
}

public interface IScanViewModel : IViewModel, IInitializableViewModel
{
    ImageSource? PhotoPreview { get; }
    bool HasPhoto { get; }
    bool HasResult { get; }
    bool HasMatches { get; }
    string PredictionSummary { get; }
    string ConfidenceMessage { get; }
    string ConfirmationMessage { get; }
    IReadOnlyList<ItemCategory> Categories { get; }
    ItemCategory? SelectedCategory { get; set; }
    ObservableCollection<PredictionDto> Predictions { get; }
    ObservableCollection<PackingItemDto> MatchingItems { get; }
    PackingItemDto? SelectedItem { get; set; }
    IAsyncRelayCommand CaptureCommand { get; }
    IAsyncRelayCommand PickCommand { get; }
    IAsyncRelayCommand ConfirmCommand { get; }
    IAsyncRelayCommand AddItemCommand { get; }
}

public interface ISettingsViewModel : IViewModel
{
    string ApiBaseUrl { get; set; }
    string StatusMessage { get; }
    string DefaultApiBaseUrl { get; }
    IAsyncRelayCommand SaveCommand { get; }
    IAsyncRelayCommand TestCommand { get; }
    IRelayCommand ResetCommand { get; }
}
