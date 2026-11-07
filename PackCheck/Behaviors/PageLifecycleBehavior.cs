using PackCheck.Services;

namespace PackCheck.Behaviors;

// View lifecycle glue lives here so pages keep only constructor wiring.
public class PageLifecycleBehavior : Behavior<ContentPage>
{
    protected override void OnAttachedTo(ContentPage page)
    {
        base.OnAttachedTo(page);
        page.Appearing += OnAppearing;
    }

    protected override void OnDetachingFrom(ContentPage page)
    {
        page.Appearing -= OnAppearing;
        base.OnDetachingFrom(page);
    }

    private static void OnAppearing(object? sender, EventArgs args)
    {
        if (sender is ContentPage { BindingContext: IPageViewModel viewModel }
            && viewModel.AppearingCommand.CanExecute(null))
            viewModel.AppearingCommand.Execute(null);
    }
}
