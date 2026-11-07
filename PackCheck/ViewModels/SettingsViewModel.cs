using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PackCheck.Messages;
using PackCheck.Services;

namespace PackCheck.ViewModels;

public sealed class SettingsViewModel : BaseViewModel, ISettingsViewModel
{
    private readonly ISettingsService settings;
    private string apiBaseUrl;
    private string statusMessage = string.Empty;

    public SettingsViewModel(ISettingsService settings, ITemplateDataService templates, IMessenger messenger) : base(messenger)
    {
        this.settings = settings;
        apiBaseUrl = settings.ApiBaseUrl;
        SaveCommand = new AsyncRelayCommand(() => ExecuteAsync(() =>
        {
            if (SaveUrl()) StatusMessage = "API address saved. Return to your trips and refresh to connect.";
            return Task.CompletedTask;
        }));
        TestCommand = new AsyncRelayCommand(() => ExecuteAsync(async () =>
        {
            if (!SaveUrl()) return;
            var result = await templates.GetAllAsync();
            StatusMessage = $"Connected. The API returned {result.Count} packing templates.";
        }));
        ResetCommand = new RelayCommand(() => { ApiBaseUrl = DefaultApiBaseUrl; StatusMessage = "Default address restored. Save to apply."; });
    }

    public string ApiBaseUrl { get => apiBaseUrl; set => SetProperty(ref apiBaseUrl, value); }
    public string DefaultApiBaseUrl => settings.DefaultApiBaseUrl;
    public string StatusMessage { get => statusMessage; private set => SetProperty(ref statusMessage, value); }
    public IAsyncRelayCommand SaveCommand { get; }
    public IAsyncRelayCommand TestCommand { get; }
    public IRelayCommand ResetCommand { get; }

    private bool SaveUrl()
    {
        StatusMessage = string.Empty;
        if (!Uri.TryCreate(ApiBaseUrl.Trim(), UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp)
            || !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment)
            || !string.IsNullOrEmpty(uri.UserInfo))
        {
            ErrorMessage = "Enter an absolute http:// or https:// API address without a query or fragment.";
            return false;
        }
        var previous = settings.ApiBaseUrl;
        settings.ApiBaseUrl = uri.AbsoluteUri.TrimEnd('/') + "/";
        ApiBaseUrl = settings.ApiBaseUrl;
        if (!string.Equals(previous, settings.ApiBaseUrl, StringComparison.Ordinal))
            Messenger.Send(new ApiAddressChangedMessage(settings.ApiBaseUrl));
        return true;
    }
}
