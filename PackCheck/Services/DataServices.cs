using PackCheck.Contracts;

namespace PackCheck.Services;

public class TripDataService(HttpClient httpClient, ISettingsService settings)
    : ApiService<TripDto>(httpClient, settings), ITripDataService
{
    protected override string EndPoint => "trips";
    public Task<TripDto> CreateAsync(CreateTripRequest request) => PostAsync(request);
}

public class PackingItemDataService(HttpClient httpClient, ISettingsService settings)
    : ApiService<PackingItemDto>(httpClient, settings), IPackingItemDataService
{
    protected override string EndPoint => "trips";
    private static string ItemsPath(int tripId) => $"trips/{tripId}/items";
    public Task<List<PackingItemDto>> GetAllAsync(int tripId) => GetResourceAsync<List<PackingItemDto>>(ItemsPath(tripId));
    public Task<PackingItemDto> GetAsync(int tripId, int itemId) => GetResourceAsync<PackingItemDto>($"{ItemsPath(tripId)}/{itemId}");
    public Task<PackingItemDto> CreateAsync(int tripId, SavePackingItemRequest request) =>
        SendResourceAsync<PackingItemDto, SavePackingItemRequest>(HttpMethod.Post, ItemsPath(tripId), request);
    public Task<PackingItemDto> UpdateAsync(int tripId, int itemId, SavePackingItemRequest request) =>
        SendResourceAsync<PackingItemDto, SavePackingItemRequest>(HttpMethod.Put, $"{ItemsPath(tripId)}/{itemId}", request);
    public Task DeleteAsync(int tripId, int itemId) => DeleteResourceAsync($"{ItemsPath(tripId)}/{itemId}");
}

public class TemplateDataService(HttpClient httpClient, ISettingsService settings)
    : ApiService<PackingTemplateDto>(httpClient, settings), ITemplateDataService
{
    protected override string EndPoint => "templates";
}

public class RecognitionService(HttpClient httpClient, ISettingsService settings)
    : ApiService<RecognitionResultDto>(httpClient, settings), IRecognitionService
{
    protected override string EndPoint => "recognition";

    public async Task<RecognitionResultDto> ClassifyImageAsync(PhotoUpload photo)
    {
        using var form = new MultipartFormDataContent();
        using var content = new ByteArrayContent(photo.Bytes);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(photo.ContentType);
        form.Add(content, "image", photo.FileName);
        using var response = await RequestAsync(() => HttpClient.PostAsync(BuildUri(EndPoint), form));
        return await ReadResponseAsync<RecognitionResultDto>(response);
    }
}
