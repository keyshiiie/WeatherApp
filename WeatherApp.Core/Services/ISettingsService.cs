using System;
using System.Collections.Generic;
using System.Text;
using WeatherApp.Core.Models;
using WeatherApp.Core.Results;

namespace WeatherApp.Core.Services
{
    public interface ISettingsService
    {
        Result<UserSettings> GetSettings();
        Result SaveSettings(UserSettings settings);
        Result SetTemperatureUnit(TemperatureUnit unit);
        Result SetPressureUnit(PressureUnit unit);
        Result SetSpeedUnit(SpeedUnit unit);
        Result SetThemeMode(ThemeMode theme);
        event EventHandler<UserSettings>? SettingsChanged;
    }
}
