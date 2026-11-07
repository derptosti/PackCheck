using Microsoft.Extensions.DependencyInjection;

namespace PackCheck
{
    public partial class App : Application
    {
        private readonly IServiceProvider services;

        public App(IServiceProvider services)
        {
            InitializeComponent();
            this.services = services;
            UserAppTheme = AppTheme.Light;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Resolve views after application resources have been initialized.
            return new Window(services.GetRequiredService<AppShell>());
        }
    }
}
