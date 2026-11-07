namespace PackCheck
{
    public partial class AppShell : Shell
    {
        public AppShell(PackCheck.Views.TripsPage tripsPage)
        {
            InitializeComponent();
            TripsContent.Content = tripsPage;
        }
    }
}
