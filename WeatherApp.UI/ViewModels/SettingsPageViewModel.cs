using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WeatherApp.Core.Models;
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

    public List<TemperatureUnit> TemperatureUnits { get; } =
        Enum.GetValues(typeof(TemperatureUnit)).Cast<TemperatureUnit>().ToList();

    public List<PressureUnit> PressureUnits { get; } =
        Enum.GetValues(typeof(PressureUnit)).Cast<PressureUnit>().ToList();

    public List<SpeedUnit> SpeedUnits { get; } =
        Enum.GetValues(typeof(SpeedUnit)).Cast<SpeedUnit>().ToList();

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

        var settings = _settingsService.GetSettings();
        SelectedTemperatureUnit = settings.TemperatureUnit;
        SelectedPressureUnit = settings.PressureUnit;
        SelectedSpeedUnit = settings.SpeedUnit;

        Logger.LogInformation($"Settings loaded: Temp={SelectedTemperatureUnit}, Pressure={SelectedPressureUnit}, Speed={SelectedSpeedUnit}");
    }

    partial void OnSelectedTemperatureUnitChanged(TemperatureUnit value)
    {
        Logger.LogInformation($"Temperature unit changed to: {value}");
        _settingsService.SetTemperatureUnit(value);
    }

    partial void OnSelectedPressureUnitChanged(PressureUnit value)
    {
        Logger.LogInformation($"Pressure unit changed to: {value}");
        _settingsService.SetPressureUnit(value);
    }

    partial void OnSelectedSpeedUnitChanged(SpeedUnit value)
    {
        Logger.LogInformation($"Speed unit changed to: {value}");
        _settingsService.SetSpeedUnit(value);
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
            await Shell.Current.CurrentPage.DisplayAlertAsync("Ошибка", "Не удалось открыть ссылку", "ОК");
        }
    }

    [RelayCommand]
    private async Task OpenFeedbackLink()
    {
        Logger.LogInformation("Opening PrivacyPolicy link");

        try
        {
            await Launcher.Default.OpenAsync("https://docs.google.com/document/d/1M5El2eHB7WYWytcqArusLCM1xe7VtZUb59o_I7BAuQA");
            Logger.LogInformation("Link opened successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error opening link");
            await Shell.Current.CurrentPage.DisplayAlertAsync("Ошибка", "Не удалось открыть ссылку", "ОК");
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
            await Shell.Current.CurrentPage.DisplayAlertAsync("Ошибка", "Не удалось открыть ссылку", "ОК");
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
            await Shell.Current.CurrentPage.DisplayAlertAsync("Ошибка", "Не удалось открыть страницу смены ключа", "ОК");
        }
    }
}