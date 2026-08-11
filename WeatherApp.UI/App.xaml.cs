using WeatherApp.Core.Services;

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