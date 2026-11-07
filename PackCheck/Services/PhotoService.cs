namespace PackCheck.Services;

public class PhotoService : IPhotoService
{
    private const int MaximumBytes = 4 * 1024 * 1024;

    public async Task<PhotoUpload?> PickAsync()
    {
        var files = await MediaPicker.Default.PickPhotosAsync(CreateOptions("Choose one packing item"));
        var file = files.FirstOrDefault();
        return file is null ? null : await ReadPhotoAsync(file);
    }

    public async Task<PhotoUpload?> CaptureAsync()
    {
        if (!MediaPicker.Default.IsCaptureSupported)
            throw new InvalidOperationException("A camera is unavailable on this device. Use Choose photo instead.");

        var file = await MediaPicker.Default.CapturePhotoAsync(CreateOptions("Photograph one item"));
        return file is null ? null : await ReadPhotoAsync(file);
    }

    private static MediaPickerOptions CreateOptions(string title) => new()
    {
        Title = title,
        SelectionLimit = 1,
        MaximumWidth = 1600,
        MaximumHeight = 1600,
        CompressionQuality = 85,
        RotateImage = true,
        PreserveMetaData = false
    };

    private static async Task<PhotoUpload> ReadPhotoAsync(FileResult file)
    {
        // Bound the read even when a platform picker does not report the file length.
        using var stream = await file.OpenReadAsync();
        using var output = new MemoryStream();
        var buffer = new byte[81920];
        int read;
        while ((read = await stream.ReadAsync(buffer)) > 0)
        {
            if (output.Length + read > MaximumBytes)
                throw new InvalidOperationException("Choose a photo smaller than 4 MB. Try a lower camera resolution.");
            await output.WriteAsync(buffer.AsMemory(0, read));
        }

        var bytes = output.ToArray();
        var contentType = DetectContentType(bytes);
        if (contentType is null)
            throw new InvalidOperationException("Choose a JPEG, PNG, GIF or BMP photo. Convert HEIC photos to JPEG first.");

        return new PhotoUpload(bytes, Path.GetFileName(file.FileName), contentType);
    }

    private static string? DetectContentType(byte[] bytes)
    {
        if (bytes.Length < 8) return null;
        if (bytes[0] == 0xff && bytes[1] == 0xd8 && bytes[2] == 0xff) return "image/jpeg";
        if (bytes.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })) return "image/png";
        if (bytes[0] == 'G' && bytes[1] == 'I' && bytes[2] == 'F' && bytes[3] == '8') return "image/gif";
        if (bytes[0] == 'B' && bytes[1] == 'M') return "image/bmp";
        return null;
    }
}
