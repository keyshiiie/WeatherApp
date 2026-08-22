using Microsoft.Extensions.Logging;
using WeatherApp.Core.Services;
using WeatherApp.UI.Views;

namespace WeatherApp.UI;

public partial class App : Application
{
    private readonly IWeatherService _weatherService;
    private readonly ILogger<App> _logger;

    public App(IWeatherService weatherService, ILogger<App> logger)
    {
        InitializeComponent();
        _weatherService = weatherService;
        _logger = logger;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }

    protected override async void OnStart()
    {
        base.OnStart();

        try
        {
            var apiKey = await SecureStorage.GetAsync("weather_api_key");

            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("API key not found, redirecting to LoginPage");
                await Shell.Current.GoToAsync(nameof(LoginPage));
                return;
            }

            _logger.LogInformation("API key is valid");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during app start");
            await Shell.Current.CurrentPage.DisplayAlertAsync(
                "Ошибка",
                "Не удалось проверить API ключ. Проверьте подключение к интернету.",
                "ОК");
        }
    }
}