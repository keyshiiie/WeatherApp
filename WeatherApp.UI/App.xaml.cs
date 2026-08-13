using Microsoft.Maui.Storage;
using WeatherApp.Core.Services;
using WeatherApp.UI.Views;

namespace WeatherApp.UI;

public partial class App : Application
{
    private readonly WeatherAlertService _weatherAlertService;

    public App(WeatherAlertService weatherAlertService)
    {
        InitializeComponent();
        _weatherAlertService = weatherAlertService;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }

    protected override async void OnStart()
    {
        base.OnStart();

        // Проверяем наличие ключа (без блокировки UI)
        var apiKey = await SecureStorage.GetAsync("weather_api_key");

        if (string.IsNullOrEmpty(apiKey))
        {
            // Если ключа нет -> отправляем на страницу логина через Shell!
            await Shell.Current.GoToAsync(nameof(LoginPage));
        }

        try
        {
            // await _weatherAlertService.CheckAndNotifyAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка при проверке уведомлений: {ex.Message}");
        }
    }
}