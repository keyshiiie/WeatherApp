using Microsoft.Maui.Storage;
using WeatherApp.Core.Models;
using WeatherApp.Core.Services;

public class SettingsService : ISettingsService
{
    private const string TemperatureKey = "temperature_unit";
    private const string PressureKey = "pressure_unit";
    private const string SpeedKey = "speed_unit";

    private UserSettings _cachedSettings;
    private readonly object _lock = new();

    public event EventHandler<UserSettings>? SettingsChanged;

    public SettingsService()
    {
        _cachedSettings = LoadSettings();
    }

    public UserSettings GetSettings()
    {
        lock (_lock)
        {
            return _cachedSettings;
        }
    }

    public void SaveSettings(UserSettings settings)
    {
        lock (_lock)
        {
            Preferences.Set(TemperatureKey, (int)settings.TemperatureUnit);
            Preferences.Set(PressureKey, (int)settings.PressureUnit);
            Preferences.Set(SpeedKey, (int)settings.SpeedUnit);
            _cachedSettings = settings;
        }
        // Вызываем событие ПОСЛЕ сохранения
        SettingsChanged?.Invoke(this, settings);
    }

    public void SetTemperatureUnit(TemperatureUnit unit)
    {
        var settings = GetSettings();
        settings.TemperatureUnit = unit;
        SaveSettings(settings); // Здесь вызывается SettingsChanged
    }

    public void SetPressureUnit(PressureUnit unit)
    {
        var settings = GetSettings();
        settings.PressureUnit = unit;
        SaveSettings(settings); // Здесь вызывается SettingsChanged
    }

    public void SetSpeedUnit(SpeedUnit unit)
    {
        var settings = GetSettings();
        settings.SpeedUnit = unit;
        SaveSettings(settings); // Здесь вызывается SettingsChanged
    }

    private UserSettings LoadSettings()
    {
        return new UserSettings
        {
            TemperatureUnit = (TemperatureUnit)Preferences.Get(TemperatureKey, (int)TemperatureUnit.Celsius),
            PressureUnit = (PressureUnit)Preferences.Get(PressureKey, (int)PressureUnit.Millibars),
            SpeedUnit = (SpeedUnit)Preferences.Get(SpeedKey, (int)SpeedUnit.KilometersPerHour)
        };
    }
}