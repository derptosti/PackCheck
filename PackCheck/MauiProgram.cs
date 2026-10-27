using Microsoft.Extensions.Logging;
using PackCheck.ViewModels;
using PackCheck.ViewModels.Interfaces;
using PackCheck.Views;

namespace PackCheck
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif
            builder.Services.AddTransient<ITripsViewModel, TripsViewModel>();
            builder.Services.AddTransient<TripsPage>();

            return builder.Build();
        }
    }
}
