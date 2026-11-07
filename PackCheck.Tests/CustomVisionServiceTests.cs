using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using PackCheck.Api.Infrastructure;
using PackCheck.Api.Services;
using Xunit;

namespace PackCheck.Tests;

public sealed class CustomVisionServiceTests
{
    [Fact]
    public async Task SendsThePublishedModelUrlPredictionKeyAndOriginalImageBytes()
    {
        byte[] image = [0xff, 0xd8, 0xff, 1, 2, 3];
        var handler = new StubHandler(async (request, cancellationToken) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("https://vision.example.test/customvision/v3.1/prediction/11111111-2222-3333-4444-555555555555/classify/iterations/packing%20v1/image", request.RequestUri!.AbsoluteUri);
            Assert.Equal("test-prediction-key", Assert.Single(request.Headers.GetValues("Prediction-Key")));
            Assert.Equal("application/octet-stream", request.Content!.Headers.ContentType!.MediaType);
            Assert.Equal(image, await request.Content.ReadAsByteArrayAsync(cancellationToken));
            return Json("""{"predictions":[{"tagName":"torch","probability":0.9}]}""");
        });
        using var http = new HttpClient(handler);
        var service = new CustomVisionService(http, Options.Create(Configured()));

        var result = await service.ClassifyImageAsync(image, CancellationToken.None);

        Assert.Equal(1, handler.Calls);
        Assert.Equal("torch", Assert.Single(result.Predictions).CategoryKey);
        Assert.Equal(0.65, result.ConfidenceThreshold);
    }

    [Fact]
    public async Task PreservesNegativeAsTopResultRatherThanPromotingAWeakSupportedPrediction()
    {
        var handler = new StubHandler((_, _) => Task.FromResult(Json("""
            {"predictions":[
              {"tagName":"water bottle","probability":0.15},
              {"tagName":"Negative","probability":0.75},
              {"tagName":"torch","probability":0.1}
            ]}
            """)));
        using var http = new HttpClient(handler);
        var service = new CustomVisionService(http, Options.Create(Configured()));

        var result = await service.ClassifyImageAsync([1], CancellationToken.None);

        Assert.Equal(["negative", "water-bottle", "torch"], result.Predictions.Select(prediction => prediction.CategoryKey));
        Assert.False(result.Predictions[0].IsSupported);
        Assert.True(result.Predictions[1].IsSupported);
        Assert.Equal(0.15, result.Predictions[1].Probability);
    }

    [Fact]
    public async Task UnconfiguredModelReturnsUnavailableWithoutSendingARequest()
    {
        var handler = new StubHandler((_, _) => throw new InvalidOperationException("An unconfigured service must not make an HTTP request."));
        using var http = new HttpClient(handler);
        var service = new CustomVisionService(http, Options.Create(new CustomVisionOptions()));

        var error = await Assert.ThrowsAsync<ApiException>(() => service.ClassifyImageAsync([1], CancellationToken.None));

        Assert.Equal(503, error.StatusCode);
        Assert.Equal(0, handler.Calls);
    }

    [Fact]
    public async Task AzureFailureReturnsBadGatewayWithoutLeakingItsResponseBody()
    {
        var handler = new StubHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent("sensitive upstream diagnostic")
        }));
        using var http = new HttpClient(handler);
        var service = new CustomVisionService(http, Options.Create(Configured()));

        var error = await Assert.ThrowsAsync<ApiException>(() => service.ClassifyImageAsync([1], CancellationToken.None));

        Assert.Equal(502, error.StatusCode);
        Assert.DoesNotContain("sensitive upstream diagnostic", error.Message);
    }

    [Theory]
    [InlineData("not JSON")]
    [InlineData("{\"predictions\":[]}")]
    [InlineData("{\"predictions\":[{\"tagName\":\"torch\",\"probability\":1.5}]}")]
    public async Task UnusableAzureResponsesReturnBadGatewayInsteadOfInventingAPrediction(string body)
    {
        var handler = new StubHandler((_, _) => Task.FromResult(Json(body)));
        using var http = new HttpClient(handler);
        var service = new CustomVisionService(http, Options.Create(Configured()));

        var error = await Assert.ThrowsAsync<ApiException>(() => service.ClassifyImageAsync([1], CancellationToken.None));

        Assert.Equal(502, error.StatusCode);
    }

    private static CustomVisionOptions Configured() => new()
    {
        Endpoint = "https://vision.example.test/",
        ProjectId = "11111111-2222-3333-4444-555555555555",
        PublishedName = "packing v1",
        PredictionKey = "test-prediction-key",
        ConfidenceThreshold = 0.65
    };

    private static HttpResponseMessage Json(string body) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(body, Encoding.UTF8, "application/json")
    };

    private sealed class StubHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond) : HttpMessageHandler
    {
        public int Calls { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            return respond(request, cancellationToken);
        }
    }
}
