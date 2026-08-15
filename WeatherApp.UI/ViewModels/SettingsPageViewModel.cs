using CommunityToolkit.Mvvm.ComponentModel;
using WeatherApp.Core.Models;
using WeatherApp.Core.Services;

namespace WeatherApp.UI.ViewModels;

public partial class SettingsPageViewModel : ObservableObject
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

    public SettingsPageViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        LoadSettings();
    }

    private void LoadSettings()
    {
        var settings = _settingsService.GetSettings();
        SelectedTemperatureUnit = settings.TemperatureUnit;
        SelectedPressureUnit = settings.PressureUnit;
        SelectedSpeedUnit = settings.SpeedUnit;
    }

    partial void OnSelectedTemperatureUnitChanged(TemperatureUnit value)
    {
        _settingsService.SetTemperatureUnit(value);
    }

    partial void OnSelectedPressureUnitChanged(PressureUnit value)
    {
        _settingsService.SetPressureUnit(value);
    }

    partial void OnSelectedSpeedUnitChanged(SpeedUnit value)
    {
        _settingsService.SetSpeedUnit(value);
    }
}