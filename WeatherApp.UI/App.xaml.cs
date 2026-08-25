using Microsoft.Extensions.Logging;
using WeatherApp.Core.Models;
using WeatherApp.Core.Services;
using WeatherApp.UI.Services;
using WeatherApp.UI.Views;

namespace WeatherApp.UI;

public partial class App : Application
{
    private readonly IWeatherService _weatherService;
    private readonly ISettingsService _settingsService;
    private readonly INavigationService? _navigationService;
    private readonly ILogger<App> _logger;

    public App(
        IWeatherService weatherService,
        ISettingsService settingsService,
        INavigationService navigationService,
        ILogger<App> logger)
    {
        InitializeComponent();

        _weatherService = weatherService ?? throw new ArgumentNullException(nameof(weatherService));
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _navigationService = navigationService;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        ApplySavedTheme();

        _logger.LogInformation("App initialized");
    }

    private void ApplySavedTheme()
    {
        try
        {
            var settingsResult = _settingsService.GetSettings();
            if (settingsResult.IsSuccess && settingsResult.Value != null)
            {
                ApplyTheme(settingsResult.Value.ThemeMode);
                _logger.LogInformation("Theme applied: {Theme}", settingsResult.Value.ThemeMode);
            }
            else
            {
                ApplyTheme(ThemeMode.System);
                _logger.LogWarning("Failed to load settings, using system theme");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply saved theme");
            ApplyTheme(ThemeMode.System);
        }
    }

    private void ApplyTheme(ThemeMode theme)
    {
        try
        {
            UserAppTheme = theme switch
            {
                ThemeMode.Light => Microsoft.Maui.ApplicationModel.AppTheme.Light,
                ThemeMode.Dark => Microsoft.Maui.ApplicationModel.AppTheme.Dark,
                _ => Microsoft.Maui.ApplicationModel.AppTheme.Unspecified,
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to apply theme: {Theme}", theme);
        }
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

                if (_navigationService != null)
                {
                    await _navigationService.GoToLoginPageAsync();
                }
                else
                {
                    await Shell.Current.GoToAsync(nameof(LoginPage));
                }
                return;
            }

            _logger.LogInformation("API key is valid");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during app start");

            try
            {
                var currentPage = Shell.Current?.CurrentPage;
                if (currentPage != null)
                {
                    await currentPage.DisplayAlertAsync(
                        "Ошибка",
                        "Не удалось проверить API ключ. Проверьте подключение к интернету.",
                        "ОК");
                }
                else
                {
                    _logger.LogWarning("Cannot display alert: CurrentPage is null");
                }
            }
            catch (Exception alertEx)
            {
                _logger.LogError(alertEx, "Failed to display error alert");
            }
        }
    }
}