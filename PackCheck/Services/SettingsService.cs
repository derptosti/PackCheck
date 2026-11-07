namespace PackCheck.Services;

public class SettingsService : ISettingsService
{
    private const string ApiUrlKey = "packcheck.api-base-url";
    public string DefaultApiBaseUrl => DeviceInfo.Platform == DevicePlatform.Android
        ? "http://10.0.2.2:5092/" : "http://localhost:5092/";

    public string ApiBaseUrl
    {
        get => Preferences.Default.Get(ApiUrlKey, DefaultApiBaseUrl);
        set
        {
            if (!Uri.TryCreate(value?.Trim(), UriKind.Absolute, out var uri)
                || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp)
                || !string.IsNullOrEmpty(uri.UserInfo) || !string.IsNullOrEmpty(uri.Query)
                || !string.IsNullOrEmpty(uri.Fragment) || uri.AbsolutePath != "/")
            {
                throw new ArgumentException("Enter the API's base URL, for example https://your-tunnel.ngrok-free.app, without /api or a path.");
            }

            Preferences.Default.Set(ApiUrlKey, uri.AbsoluteUri);
        }
    }
}
