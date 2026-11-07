namespace PackCheck.Services;

public class DialogService : IDialogService
{
    public Task<bool> ConfirmAsync(string title, string message) =>
        Shell.Current.DisplayAlertAsync(title, message, "Delete", "Cancel");
}
