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

            var isValid = await ValidateApiKeyAsync(apiKey);

            if (!isValid)
            {
                _logger.LogWarning("API key is invalid, redirecting to LoginPage");
                await Shell.Current.CurrentPage.DisplayAlertAsync(
                    "Ошибка API ключа",
                    "Ваш API ключ недействителен. Пожалуйста, введите новый ключ.",
                    "ОК");
                await SecureStorage.SetAsync("weather_api_key", string.Empty);
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

    private async Task<bool> ValidateApiKeyAsync(string apiKey)
    {
        try
        {
            var weather = await _weatherService.GetCurrentWeatherAsync("London");
            return weather != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API key validation failed");
            return false;
        }
    }
}