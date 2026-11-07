using CommunityToolkit.Mvvm.Input;
using PackCheck.Contracts;

namespace PackCheck.Services;

public interface IPageViewModel
{
    IAsyncRelayCommand AppearingCommand { get; }
}

public interface IInitializableViewModel
{
    void Initialize(object? parameter);
}

public record ItemNavigationParameter(int TripId, int? ItemId = null);

public interface INavigationService
{
    Task NavigateToAsync(string route, object? parameter = null);
    Task GoBackAsync();
}

public interface IApiService<T>
{
    Task<List<T>> GetAllAsync();
    Task<T> GetAsync(int id);
    Task<T> PostAsync<TRequest>(TRequest request);
    Task<T> PutAsync<TRequest>(int id, TRequest request);
    Task DeleteAsync(int id);
}

public interface ITripDataService
{
    Task<List<TripDto>> GetAllAsync();
    Task<TripDto> GetAsync(int id);
    Task<TripDto> CreateAsync(CreateTripRequest request);
    Task DeleteAsync(int id);
}

public interface IPackingItemDataService
{
    Task<List<PackingItemDto>> GetAllAsync(int tripId);
    Task<PackingItemDto> GetAsync(int tripId, int itemId);
    Task<PackingItemDto> CreateAsync(int tripId, SavePackingItemRequest request);
    Task<PackingItemDto> UpdateAsync(int tripId, int itemId, SavePackingItemRequest request);
    Task DeleteAsync(int tripId, int itemId);
}

public interface ITemplateDataService
{
    Task<List<PackingTemplateDto>> GetAllAsync();
}

public record PhotoUpload(byte[] Bytes, string FileName, string ContentType);

public interface IRecognitionService
{
    Task<RecognitionResultDto> ClassifyImageAsync(PhotoUpload photo);
}

public interface IPhotoService
{
    Task<PhotoUpload?> PickAsync();
    Task<PhotoUpload?> CaptureAsync();
}

public interface ISettingsService
{
    string ApiBaseUrl { get; set; }
    string DefaultApiBaseUrl { get; }
}

public interface IDialogService
{
    Task<bool> ConfirmAsync(string title, string message);
}
