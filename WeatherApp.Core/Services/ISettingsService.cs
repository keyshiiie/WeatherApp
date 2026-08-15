using System;
using System.Collections.Generic;
using System.Text;
using WeatherApp.Core.Models;

namespace WeatherApp.Core.Services
{
    public interface ISettingsService
    {
        UserSettings GetSettings();
        void SaveSettings(UserSettings settings);
        void SetTemperatureUnit(TemperatureUnit unit);
        void SetPressureUnit(PressureUnit unit);
        void SetSpeedUnit(SpeedUnit unit);
        event EventHandler<UserSettings>? SettingsChanged;
    }
}
