using Microsoft.AspNetCore.Mvc;
using PackCheck.Api.Infrastructure;
using PackCheck.Api.Services;
using PackCheck.Contracts;

namespace PackCheck.Api.Controllers;

[ApiController]
[Route("api/recognition")]
public class RecognitionController(IImageRecognitionService service) : ControllerBase
{
    private const int MaximumImageBytes = 4 * 1024 * 1024;

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaximumImageBytes + 64 * 1024)]
    public async Task<RecognitionResultDto> Classify([FromForm] IFormFile image, CancellationToken cancellationToken)
    {
        if (image is null || image.Length == 0)
            throw new ApiException(400, "Select a photo of one item.");
        if (image.Length > MaximumImageBytes)
            throw new ApiException(413, "Choose an image smaller than 4 MB.");
        var supportedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/bmp" };
        if (!supportedTypes.Contains(image.ContentType, StringComparer.OrdinalIgnoreCase))
            throw new ApiException(415, "Choose a JPEG, PNG, GIF, or BMP image.");

        using var buffer = new MemoryStream();
        await image.CopyToAsync(buffer, cancellationToken);
        var bytes = buffer.ToArray();
        if (!HasImageHeader(bytes, image.ContentType.ToLowerInvariant()))
            throw new ApiException(415, "The file does not match its image format. Choose a JPEG, PNG, GIF, or BMP image.");

        return await service.ClassifyImageAsync(bytes, cancellationToken);
    }

    private static bool HasImageHeader(byte[] bytes, string contentType) => contentType switch
    {
        "image/jpeg" => bytes.Length >= 3 && bytes[0] == 0xff && bytes[1] == 0xd8 && bytes[2] == 0xff,
        "image/png" => bytes.AsSpan().StartsWith(new byte[] { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a }),
        "image/gif" => bytes.AsSpan().StartsWith("GIF87a"u8) || bytes.AsSpan().StartsWith("GIF89a"u8),
        "image/bmp" => bytes.AsSpan().StartsWith("BM"u8),
        _ => false
    };
}
