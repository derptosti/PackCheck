using PackCheck.Services;

namespace PackCheck.ViewModelTests;

internal sealed class FakeNavigation : INavigationService
{
    public Task NavigateToAsync(string route, object? parameter = null) => Task.CompletedTask;
    public Task GoBackAsync() => Task.CompletedTask;
}
