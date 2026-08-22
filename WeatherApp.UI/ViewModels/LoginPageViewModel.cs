using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WeatherApp.Core.Services;

namespace WeatherApp.UI.ViewModels;

public partial class LoginPageViewModel : BaseViewModel
{
    private readonly IWeatherService _weatherService;

    [ObservableProperty]
    private string? _apiKey;

    [ObservableProperty]
    private bool _isValidating;

    public LoginPageViewModel(
        IWeatherService weatherService,
        ILogger<LoginPageViewModel> logger)
        : base(logger)
    {
        Title = "Вход";
        _weatherService = weatherService ?? throw new ArgumentNullException(nameof(weatherService));
        Logger.LogInformation("LoginPageViewModel initialized");
    }

    public override async Task OnAppearingAsync()
    {
        Logger.LogInformation("Login page appearing");

        try
        {
            var savedKey = await SecureStorage.GetAsync("weather_api_key");
            if (!string.IsNullOrEmpty(savedKey))
            {
                Logger.LogInformation("Existing API key found, validating...");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error checking for saved API key");
            await Shell.Current.CurrentPage.DisplayAlertAsync(
                "Ошибка",
                "Не удалось проверить сохраненный ключ",
                "ОК");
        }
    }

    [RelayCommand]
    private async Task Login()
    {
        Logger.LogInformation("Login attempt");

        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            Logger.LogWarning("Login failed: API key is empty");
            await Shell.Current.CurrentPage.DisplayAlertAsync("Ошибка", "Введите API ключ!", "ОК");
            return;
        }

        var trimmedKey = ApiKey.Trim();

        try
        {
            IsValidating = true;

            Logger.LogInformation("API key is valid, saving...");
            await SecureStorage.SetAsync("weather_api_key", trimmedKey);

            Logger.LogInformation("API key saved successfully, navigating back");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error validating/saving API key");
            await Shell.Current.CurrentPage.DisplayAlertAsync(
                "Ошибка",
                "Не удалось проверить API ключ. Проверьте подключение к интернету.",
                "ОК");
        }
        finally
        {
            IsValidating = false;
        }
    }

    [RelayCommand]
    private async Task OpenLink()
    {
        Logger.LogInformation("Opening WeatherAPI signup link");

        try
        {
            await Launcher.Default.OpenAsync("https://www.weatherapi.com/signup.aspx");
            Logger.LogInformation("Link opened successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error opening link");
            await Shell.Current.CurrentPage.DisplayAlertAsync("Ошибка", "Не удалось открыть ссылку", "ОК");
        }
    }
}