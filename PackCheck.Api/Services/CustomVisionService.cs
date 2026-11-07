using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PackCheck.Api.Infrastructure;
using PackCheck.Contracts;

namespace PackCheck.Api.Services;

public class CustomVisionService(HttpClient httpClient, IOptions<CustomVisionOptions> options) : IImageRecognitionService
{
    public async Task<RecognitionResultDto> ClassifyImageAsync(byte[] image, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        if (!Uri.TryCreate(settings.Endpoint, UriKind.Absolute, out var endpoint) || endpoint.Scheme != "https"
            || !Guid.TryParse(settings.ProjectId, out var projectId) || string.IsNullOrWhiteSpace(settings.PredictionKey)
            || string.IsNullOrWhiteSpace(settings.PublishedName)
            || !double.IsFinite(settings.ConfidenceThreshold) || settings.ConfidenceThreshold is < 0 or > 1)
            throw new ApiException(503, "Image recognition is not configured. Configure the CustomVision settings on the API server. You can still pack items manually.");

        var requestUrl = $"{endpoint.AbsoluteUri.TrimEnd('/')}/customvision/v3.1/prediction/{projectId}/classify/iterations/{Uri.EscapeDataString(settings.PublishedName)}/image";
        using var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
        request.Headers.Add("Prediction-Key", settings.PredictionKey);
        request.Content = new ByteArrayContent(image);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        try
        {
            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
                throw new ApiException(502, "Azure could not classify this image. Check the published model and API configuration, or try another photo.");

            var result = await response.Content.ReadFromJsonAsync<CustomVisionResponse>(cancellationToken);
            if (result?.Predictions is null || result.Predictions.Count == 0)
                throw new ApiException(502, "Azure returned no predictions. Try another photo or select an item manually.");

            var predictions = result.Predictions
                .Where(prediction => !string.IsNullOrWhiteSpace(prediction.TagName)
                    && double.IsFinite(prediction.Probability) && prediction.Probability is >= 0 and <= 1)
                .OrderByDescending(prediction => prediction.Probability)
                .Select(prediction =>
                {
                    // Preserve unsupported/Negative results in their original probability order.
                    // Dropping them could turn a weak supported result into an incorrect confident match.
                    var key = prediction.TagName.Trim().ToLowerInvariant().Replace('_', '-').Replace(' ', '-');
                    var category = ItemCategories.All.FirstOrDefault(category => category.Key == key);
                    return new PredictionDto
                    {
                        CategoryKey = key,
                        DisplayName = category?.Name ?? prediction.TagName.Trim(),
                        Probability = prediction.Probability
                    };
                }).ToList();

            if (predictions.Count == 0)
                throw new ApiException(502, "Azure returned no usable predictions. Try another photo or select an item manually.");

            return new RecognitionResultDto { Predictions = predictions, ConfidenceThreshold = settings.ConfidenceThreshold };
        }
        catch (HttpRequestException)
        {
            throw new ApiException(503, "The image recognition service cannot be reached. Check your connection or pack the item manually.");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new ApiException(504, "Image recognition took too long. Try again or pack the item manually.");
        }
        catch (JsonException)
        {
            throw new ApiException(502, "Azure returned an unreadable prediction. Try again or select an item manually.");
        }
    }

    private sealed class CustomVisionResponse
    {
        public List<CustomVisionPrediction>? Predictions { get; set; }
    }

    private sealed class CustomVisionPrediction
    {
        public string TagName { get; set; } = string.Empty;
        public double Probability { get; set; }
    }
}
