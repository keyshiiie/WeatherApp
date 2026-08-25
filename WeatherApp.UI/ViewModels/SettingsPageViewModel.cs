using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WeatherApp.Core.Models;
using WeatherApp.Core.Services;
using WeatherApp.UI.Services;

namespace WeatherApp.UI.ViewModels;

public partial class SettingsPageViewModel : BaseViewModel
{
    private readonly ISettingsService _settingsService;

    [ObservableProperty]
    public partial TemperatureUnit SelectedTemperatureUnit { get; set; }

    [ObservableProperty]
    public partial PressureUnit SelectedPressureUnit { get; set; }

    [ObservableProperty]
    public partial SpeedUnit SelectedSpeedUnit { get; set; }

    [ObservableProperty]
    public partial ThemeMode SelectedThemeMode { get; set; }

    public List<TemperatureUnit> TemperatureUnits { get; } =
        Enum.GetValues<TemperatureUnit>().Cast<TemperatureUnit>().ToList();

    public List<PressureUnit> PressureUnits { get; } =
        Enum.GetValues<PressureUnit>().Cast<PressureUnit>().ToList();

    public List<SpeedUnit> SpeedUnits { get; } =
        Enum.GetValues<SpeedUnit>().Cast<SpeedUnit>().ToList();

    public List<ThemeMode> ThemeModes { get; } =
        Enum.GetValues<ThemeMode>().Cast<ThemeMode>().ToList();

    public SettingsPageViewModel(
        ISettingsService settingsService,
        INavigationService navigationService,
        ILogger<SettingsPageViewModel> logger)
        : base(logger, navigationService)
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

            Logger.LogInformation("Settings loaded: Temp={TemperatureUnit}, Pressure={PressureUnit}, Speed={SpeedUnit}, Theme={ThemeMode}",
                SelectedTemperatureUnit, SelectedPressureUnit, SelectedSpeedUnit, SelectedThemeMode);
        }
        else
        {
            Logger.LogWarning("Failed to load settings: {ErrorMessage}", result.Error?.Message);
            SetError(result.Error!);
        }
    }

    partial void OnSelectedTemperatureUnitChanged(TemperatureUnit value)
    {
        if (!IsBusy)
        {
            Logger.LogInformation("Temperature unit changed to: {Unit}", value);
            var result = _settingsService.SetTemperatureUnit(value);
            if (result.IsFailure)
            {
                Logger.LogWarning("Failed to set temperature unit: {ErrorMessage}", result.Error?.Message);
                SetError(result.Error!);
            }
        }
    }

    partial void OnSelectedPressureUnitChanged(PressureUnit value)
    {
        if (!IsBusy)
        {
            Logger.LogInformation("Pressure unit changed to: {Unit}", value);
            var result = _settingsService.SetPressureUnit(value);
            if (result.IsFailure)
            {
                Logger.LogWarning("Failed to set pressure unit: {ErrorMessage}", result.Error?.Message);
                SetError(result.Error!);
            }
        }
    }

    partial void OnSelectedSpeedUnitChanged(SpeedUnit value)
    {
        if (!IsBusy)
        {
            Logger.LogInformation("Speed unit changed to: {Unit}", value);
            var result = _settingsService.SetSpeedUnit(value);
            if (result.IsFailure)
            {
                Logger.LogWarning("Failed to set speed unit: {ErrorMessage}", result.Error?.Message);
                SetError(result.Error!);
            }
        }
    }

    partial void OnSelectedThemeModeChanged(ThemeMode value)
    {
        if (!IsBusy)
        {
            Logger.LogInformation("Theme changed to: {Theme}", value);
            var result = _settingsService.SetThemeMode(value);
            if (result.IsFailure)
            {
                Logger.LogWarning("Failed to set theme: {ErrorMessage}", result.Error?.Message);
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
            await NavigationService.GoToChangeApiKeyPageAsync();
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

            Logger.LogInformation("Theme applied: {Theme}", theme);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to apply theme: {Theme}", theme);
        }
    }
}