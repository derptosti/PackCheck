using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace PackCheck.Services;

// The course's generic CRUD service, shared by the domain data services.
public abstract class ApiService<T>(HttpClient httpClient, ISettingsService settings) : IApiService<T>
{
    protected abstract string EndPoint { get; }
    protected HttpClient HttpClient { get; } = httpClient;

    protected Uri BuildUri(string path)
    {
        var baseUri = new Uri(settings.ApiBaseUrl.TrimEnd('/') + "/", UriKind.Absolute);
        return new Uri(baseUri, "api/" + path.TrimStart('/'));
    }

    public virtual Task<List<T>> GetAllAsync() => GetResourceAsync<List<T>>(EndPoint);
    public virtual Task<T> GetAsync(int id) => GetResourceAsync<T>($"{EndPoint}/{id}");
    public virtual Task<T> PostAsync<TRequest>(TRequest request) => SendResourceAsync<T, TRequest>(HttpMethod.Post, EndPoint, request);
    public virtual Task<T> PutAsync<TRequest>(int id, TRequest request) => SendResourceAsync<T, TRequest>(HttpMethod.Put, $"{EndPoint}/{id}", request);
    public virtual Task DeleteAsync(int id) => DeleteResourceAsync($"{EndPoint}/{id}");

    protected async Task<TResponse> GetResourceAsync<TResponse>(string path)
    {
        using var response = await RequestAsync(() => HttpClient.GetAsync(BuildUri(path)));
        return await ReadResponseAsync<TResponse>(response);
    }

    protected async Task<TResponse> SendResourceAsync<TResponse, TRequest>(HttpMethod method, string path, TRequest data)
    {
        using var request = new HttpRequestMessage(method, BuildUri(path)) { Content = JsonContent.Create(data) };
        using var response = await RequestAsync(() => HttpClient.SendAsync(request));
        return await ReadResponseAsync<TResponse>(response);
    }

    protected async Task DeleteResourceAsync(string path)
    {
        using var response = await RequestAsync(() => HttpClient.DeleteAsync(BuildUri(path)));
        await EnsureSuccessAsync(response);
    }

    protected static async Task<TResponse> ReadResponseAsync<TResponse>(HttpResponseMessage response)
    {
        await EnsureSuccessAsync(response);
        try
        {
            return await response.Content.ReadFromJsonAsync<TResponse>()
                ?? throw new InvalidOperationException("The API returned an empty response. Please try again.");
        }
        catch (JsonException)
        {
            throw new InvalidOperationException("The server did not return PackCheck data. Check the API address in Settings and make sure your API and tunnel are running.");
        }
    }

    protected static async Task<HttpResponseMessage> RequestAsync(Func<Task<HttpResponseMessage>> send)
    {
        try { return await send(); }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("Cannot reach the API. Start PackCheck.Api, check your connection and verify the API address in Settings.");
        }
        catch (TaskCanceledException)
        {
            throw new InvalidOperationException("The API took too long to respond. Check your connection and try again.");
        }
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var message = response.StatusCode switch
        {
            HttpStatusCode.NotFound => "This trip or item no longer exists. Refresh the list and try again.",
            HttpStatusCode.ServiceUnavailable => "Photo recognition is unavailable. Configure and publish the Azure model on the API, or pack items manually.",
            HttpStatusCode.RequestEntityTooLarge => "Choose a photo smaller than 4 MB.",
            _ => $"The API could not complete the request ({(int)response.StatusCode}). Please try again."
        };

        try
        {
            using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            if (response.StatusCode == HttpStatusCode.BadRequest && problem.RootElement.TryGetProperty("errors", out var errors))
            {
                var validation = errors.EnumerateObject().SelectMany(error => error.Value.EnumerateArray())
                    .Select(error => error.GetString()).Where(error => !string.IsNullOrWhiteSpace(error));
                message = string.Join(" ", validation);
            }
            else if ((int)response.StatusCode < 500 && problem.RootElement.TryGetProperty("detail", out var detail)
                     && !string.IsNullOrWhiteSpace(detail.GetString()))
            {
                message = detail.GetString()!;
            }
        }
        catch (JsonException)
        {
            // An expired tunnel can return HTML instead of the API's ProblemDetails.
        }

        throw new InvalidOperationException(message);
    }
}
