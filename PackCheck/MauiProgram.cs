using Microsoft.Extensions.Logging;

using CommunityToolkit.Mvvm.Messaging;
using PackCheck.Services;
using PackCheck.ViewModels;
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

            builder.Services.AddSingleton<AppShell>();
            builder.Services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);
            builder.Services.AddSingleton<ISettingsService, SettingsService>();
            builder.Services.AddSingleton<INavigationService, NavigationService>();
            builder.Services.AddSingleton<IDialogService, DialogService>();
            builder.Services.AddSingleton<IPhotoService, PhotoService>();
            builder.Services.AddSingleton(_ =>
            {
                var client = new HttpClient { Timeout = TimeSpan.FromSeconds(45) };
                client.DefaultRequestHeaders.Add("ngrok-skip-browser-warning", "true");
                return client;
            });
            builder.Services.AddSingleton<ITripDataService, TripDataService>();
            builder.Services.AddSingleton<IPackingItemDataService, PackingItemDataService>();
            builder.Services.AddSingleton<ITemplateDataService, TemplateDataService>();
            builder.Services.AddSingleton<IRecognitionService, RecognitionService>();
            builder.Services.AddSingleton<ITripsViewModel, TripsViewModel>();
            builder.Services.AddSingleton<TripsPage>();
            builder.Services.AddTransient<ICreateTripViewModel, CreateTripViewModel>();
            builder.Services.AddTransient<CreateTripPage>();
            builder.Services.AddTransient<ITripDetailViewModel, TripDetailViewModel>();
            builder.Services.AddTransient<TripDetailPage>();
            builder.Services.AddTransient<IPackingItemViewModel, PackingItemViewModel>();
            builder.Services.AddTransient<PackingItemPage>();
            builder.Services.AddTransient<IScanViewModel, ScanViewModel>();
            builder.Services.AddTransient<ScanPage>();
            builder.Services.AddTransient<ISettingsViewModel, SettingsViewModel>();
            builder.Services.AddTransient<SettingsPage>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
