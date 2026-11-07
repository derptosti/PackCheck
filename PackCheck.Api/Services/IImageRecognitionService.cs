using PackCheck.Contracts;

namespace PackCheck.Api.Services;

public interface IImageRecognitionService
{
    Task<RecognitionResultDto> ClassifyImageAsync(byte[] image, CancellationToken cancellationToken);
}
