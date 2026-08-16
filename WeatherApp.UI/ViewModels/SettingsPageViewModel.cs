using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using WeatherApp.Core.Models;
using WeatherApp.Core.Services;
using WeatherApp.UI.ViewModels;

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
        ILogger<SettingsPageViewModel> logger) // Добавляем логгер
        : base(logger) // Передаем в базовый класс
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
}