using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WeatherApp.Core.Models;
using WeatherApp.Core.Results;
using WeatherApp.Core.Services;
using WeatherApp.UI.Views;

namespace WeatherApp.UI.ViewModels;

public partial class SettingsPageViewModel : BaseViewModel
{
    private readonly ISettingsService _settingsService;

    [ObservableProperty]
    private TemperatureUnit _selectedTemperatureUnit;

    [ObservableProperty]
    private PressureUnit _selectedPressureUnit;

    [ObservableProperty]
    private SpeedUnit _selectedSpeedUnit;
    [ObservableProperty]
    private ThemeMode _selectedThemeMode;

    public List<TemperatureUnit> TemperatureUnits { get; } =
        Enum.GetValues(typeof(TemperatureUnit)).Cast<TemperatureUnit>().ToList();

    public List<PressureUnit> PressureUnits { get; } =
        Enum.GetValues(typeof(PressureUnit)).Cast<PressureUnit>().ToList();

    public List<SpeedUnit> SpeedUnits { get; } =
        Enum.GetValues(typeof(SpeedUnit)).Cast<SpeedUnit>().ToList();
    public List<ThemeMode> ThemeModes { get; } = 
        Enum.GetValues(typeof(ThemeMode)).Cast<ThemeMode>().ToList();

    public SettingsPageViewModel(
        ISettingsService settingsService,
        ILogger<SettingsPageViewModel> logger)
        : base(logger)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        Title = "Настройки";

        Logger.LogInformation("SettingsPageViewModel initialized");
        LoadSettings();
    }

    public override async Task OnAppearingAsync()
    {
        Logger.LogInformation("Settings page appearing");
        await Task.CompletedTask;
    }

    private void LoadSettings()
    {
        Logger.LogInformation("Loading settings");

        var result = _settingsService.GetSettings();
        if (result.IsSuccess)
        {
            var settings = result.Value!;
            SelectedTemperatureUnit = settings.TemperatureUnit;
            SelectedPressureUnit = settings.PressureUnit;
            SelectedSpeedUnit = settings.SpeedUnit;
            SelectedThemeMode = settings.ThemeMode;

            Logger.LogInformation($"Settings loaded: Temp={SelectedTemperatureUnit}, " +
                $"Pressure={SelectedPressureUnit}, " +
                $"Speed={SelectedSpeedUnit}, " + 
                $"Theme={SelectedThemeMode}");
        }
        else
        {
            Logger.LogWarning($"Failed to load settings: {result.Error?.Message}");
            SetError(result.Error!);
        }
    }

    partial void OnSelectedTemperatureUnitChanged(TemperatureUnit value)
    {
        if (!IsBusy)
        {
            Logger.LogInformation($"Temperature unit changed to: {value}");
            var result = _settingsService.SetTemperatureUnit(value);
            if (result.IsFailure)
            {
                Logger.LogWarning($"Failed to set temperature unit: {result.Error?.Message}");
                SetError(result.Error!);
            }
        }
    }

    partial void OnSelectedPressureUnitChanged(PressureUnit value)
    {
        if (!IsBusy)
        {
            Logger.LogInformation($"Pressure unit changed to: {value}");
            var result = _settingsService.SetPressureUnit(value);
            if (result.IsFailure)
            {
                Logger.LogWarning($"Failed to set pressure unit: {result.Error?.Message}");
                SetError(result.Error!);
            }
        }
    }

    partial void OnSelectedSpeedUnitChanged(SpeedUnit value)
    {
        if (!IsBusy)
        {
            Logger.LogInformation($"Speed unit changed to: {value}");
            var result = _settingsService.SetSpeedUnit(value);
            if (result.IsFailure)
            {
                Logger.LogWarning($"Failed to set speed unit: {result.Error?.Message}");
                SetError(result.Error!);
            }
        }
    }

    partial void OnSelectedThemeModeChanged(ThemeMode value)
    {
        if (!IsBusy)
        {
            Logger.LogInformation($"Theme changed to: {value}");
            var result = _settingsService.SetThemeMode(value);
            if (result.IsFailure)
            {
                Logger.LogWarning($"Failed to set theme: {result.Error?.Message}");
                SetError(result.Error!);
            }
            else
            {
                ApplyTheme(value);
            }
        }
    }

    [RelayCommand]
    private async Task OpenPrivacyPolicyLink()
    {
        Logger.LogInformation("Opening PrivacyPolicy link");

        try
        {
            await Launcher.Default.OpenAsync("https://docs.google.com/document/d/1_HLs8XS7O9jJmULp2wmO5npSthn5lGssMUEi8N4vAKA");
            Logger.LogInformation("Link opened successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error opening link");
            await ShowAlertAsync("Ошибка", "Не удалось открыть ссылку");
        }
    }

    [RelayCommand]
    private async Task OpenFeedbackLink()
    {
        Logger.LogInformation("Opening Feedback link");

        try
        {
            await Launcher.Default.OpenAsync("https://docs.google.com/document/d/1M5El2eHB7WYWytcqArusLCM1xe7VtZUb59o_I7BAuQA");
            Logger.LogInformation("Link opened successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error opening link");
            await ShowAlertAsync("Ошибка", "Не удалось открыть ссылку");
        }
    }

    [RelayCommand]
    private async Task OpenSignupLink()
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
            await ShowAlertAsync("Ошибка", "Не удалось открыть ссылку");
        }
    }

    [RelayCommand]
    private async Task ChangeApiKey()
    {
        try
        {
            await Shell.Current.GoToAsync(nameof(ChangeApiKeyPage));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error navigating to ChangeApiKeyPage");
            await ShowAlertAsync("Ошибка", "Не удалось открыть страницу смены ключа");
        }
    }

    private void ApplyTheme(ThemeMode theme)
    {
        try
        {
            Application.Current?.Dispatcher.Dispatch(() =>
            {
                switch (theme)
                {
                    case ThemeMode.Light:
                        Application.Current!.UserAppTheme = Microsoft.Maui.ApplicationModel.AppTheme.Light;
                        break;
                    case ThemeMode.Dark:
                        Application.Current!.UserAppTheme = Microsoft.Maui.ApplicationModel.AppTheme.Dark;
                        break;
                    case ThemeMode.System:
                    default:
                        Application.Current!.UserAppTheme = Microsoft.Maui.ApplicationModel.AppTheme.Unspecified;
                        break;
                }
            });

            Logger.LogInformation($"Theme applied: {theme}");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Failed to apply theme: {theme}");
        }
    }
}