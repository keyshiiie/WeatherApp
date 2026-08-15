using WeatherApp.UI.Views;

namespace WeatherApp.UI
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(CurrentWeatherPage), typeof(CurrentWeatherPage));
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(ForecastPage), typeof(ForecastPage));
            Routing.RegisterRoute(nameof(DetailsPage), typeof(DetailsPage));
        }
    }
}