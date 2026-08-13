using WeatherApp.UI.Views;

namespace WeatherApp.UI
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            RegisterRoutes();
        }

        private void RegisterRoutes()
        {
            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
            Routing.RegisterRoute(nameof(CurrentWeatherPage), typeof(CurrentWeatherPage));
            Routing.RegisterRoute(nameof(ForecastPage), typeof(ForecastPage));
            Routing.RegisterRoute(nameof(DetailsPage), typeof(DetailsPage));
            Routing.RegisterRoute(nameof(FavoritesPage), typeof(FavoritesPage));
            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));

        }
    }

}
