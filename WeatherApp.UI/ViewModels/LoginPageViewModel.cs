using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;
using WeatherApp.UI.ViewModels;

namespace WeatherApp.UI.ViewModels;

public partial class LoginPageViewModel : BaseViewModel
{
    [ObservableProperty]
    private string? _apiKey;

    public LoginPageViewModel(ILogger<LoginPageViewModel> logger)
        : base(logger)
    {
        Title = "Вход";
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
                Logger.LogInformation("Existing API key found, navigating back");
                await Shell.Current.GoToAsync("..");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error checking for saved API key");
        }

        await Task.CompletedTask;
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

        try
        {
            Logger.LogInformation("Saving API key");
            await SecureStorage.SetAsync("weather_api_key", ApiKey.Trim());

            Logger.LogInformation("API key saved successfully, navigating back");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving API key");
            await Shell.Current.CurrentPage.DisplayAlertAsync("Ошибка", "Не удалось сохранить API ключ", "ОК");
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