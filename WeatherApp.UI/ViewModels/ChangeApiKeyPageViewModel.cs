using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WeatherApp.Core.Services;

namespace WeatherApp.UI.ViewModels;

public partial class ChangeApiKeyPageViewModel : BaseViewModel
{
    private readonly IWeatherService _weatherService;

    [ObservableProperty]
    private string? _currentApiKey;

    [ObservableProperty]
    private string? _newApiKey;

    [ObservableProperty]
    private string? _currentApiKeyMasked;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isLoading;

    public bool CanSave => !string.IsNullOrWhiteSpace(NewApiKey) && NewApiKey != CurrentApiKey;

    public ChangeApiKeyPageViewModel(
        IWeatherService weatherService,
        ILogger<ChangeApiKeyPageViewModel> logger)
        : base(logger)
    {
        Title = "API ключ";
        _weatherService = weatherService ?? throw new ArgumentNullException(nameof(weatherService));
    }

    public override async Task OnAppearingAsync()
    {
        Logger.LogInformation("ChangeApiKeyPage appearing");
        await LoadCurrentApiKeyAsync();
    }

    private async Task LoadCurrentApiKeyAsync()
    {
        try
        {
            CurrentApiKey = await SecureStorage.GetAsync("weather_api_key");
            CurrentApiKeyMasked = MaskApiKey(CurrentApiKey);
            Logger.LogInformation("Current API key loaded");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading API key");
        }
    }

    private string? MaskApiKey(string? apiKey)
    {
        if (string.IsNullOrEmpty(apiKey))
            return "Не установлен";

        if (apiKey.Length <= 4)
            return new string('*', apiKey.Length);

        var first = apiKey[..2];
        var last = apiKey[^2..];
        var stars = new string('*', Math.Min(apiKey.Length - 4, 10));
        return $"{first}{stars}{last}";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(NewApiKey))
        {
            HasError = true;
            ErrorMessage = "Введите API ключ";
            return;
        }

        try
        {
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;

            Logger.LogInformation("Validating new API key...");

            // Валидация ключа
            var isValid = await ValidateApiKeyAsync(NewApiKey.Trim());

            if (!isValid)
            {
                HasError = true;
                ErrorMessage = "Неверный API ключ. Проверьте его корректность.";
                return;
            }

            Logger.LogInformation("Saving new API key...");
            await SecureStorage.SetAsync("weather_api_key", NewApiKey.Trim());

            Logger.LogInformation("API key saved successfully");
            await Toast.Make("API ключ успешно обновлен", ToastDuration.Short).Show();

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving API key");
            HasError = true;
            ErrorMessage = "Не удалось сохранить API ключ. Проверьте подключение к интернету.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task<bool> ValidateApiKeyAsync(string apiKey)
    {
        try
        {
            // Временно сохраняем ключ для проверки
            var oldKey = await SecureStorage.GetAsync("weather_api_key");
            await SecureStorage.SetAsync("weather_api_key", apiKey);

            var weather = await _weatherService.GetCurrentWeatherAsync("London");

            // Возвращаем старый ключ
            if (!string.IsNullOrEmpty(oldKey))
                await SecureStorage.SetAsync("weather_api_key", oldKey);
            else
                SecureStorage.Remove("weather_api_key");

            return weather != null;
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("401") || ex.Message.Contains("403"))
        {
            Logger.LogWarning("Invalid API key (401/403)");
            return false;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "API key validation error");
            return false;
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        Logger.LogInformation("Cancelling API key change");
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task OpenLinkAsync()
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
            await Toast.Make("Не удалось открыть ссылку", ToastDuration.Short).Show();
        }
    }
}