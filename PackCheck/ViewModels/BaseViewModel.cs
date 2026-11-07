using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PackCheck.Services;

namespace PackCheck.ViewModels;

public abstract class BaseViewModel : ObservableRecipient, IPageViewModel
{
    private bool isBusy;
    private bool appearancePending;
    private string errorMessage = string.Empty;

    protected BaseViewModel(IMessenger messenger) : base(messenger)
    {
        AppearingCommand = new AsyncRelayCommand(async () =>
        {
            IsActive = true;
            if (IsBusy)
            {
                appearancePending = true;
                return;
            }
            await ExecuteAsync(OnAppearingAsync);
        }, AsyncRelayCommandOptions.AllowConcurrentExecutions);
    }

    public bool IsBusy
    {
        get => isBusy;
        protected set => SetProperty(ref isBusy, value);
    }

    public string ErrorMessage
    {
        get => errorMessage;
        protected set
        {
            if (SetProperty(ref errorMessage, value)) OnPropertyChanged(nameof(HasError));
        }
    }

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
    public IAsyncRelayCommand AppearingCommand { get; }
    protected virtual Task OnAppearingAsync() => Task.CompletedTask;

    protected async Task ExecuteAsync(Func<Task> operation)
    {
        if (IsBusy) return;
        IsBusy = true;
        ErrorMessage = string.Empty;
        try { await operation(); }
        catch (PermissionException)
        {
            ErrorMessage = "Photo access was denied. Allow camera or photo access in your device settings, or pack items manually.";
        }
        catch (OperationCanceledException)
        {
            ErrorMessage = string.Empty;
        }
        catch (Exception exception) { ErrorMessage = exception.Message; }
        finally
        {
            IsBusy = false;
            if (appearancePending)
            {
                appearancePending = false;
                await ExecuteAsync(OnAppearingAsync);
            }
        }
    }
}
