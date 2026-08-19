using System;
using System.Collections.Generic;
using System.Text;
using WeatherApp.Core.Models;

namespace WeatherApp.UI.ViewModels
{
    /// <summary>
    /// Модель для отображения почасового прогноза
    /// </summary>
    public class HourlyForecastDisplay
    {
        private readonly HourlyForecast _hour;
        private readonly UserSettings _settings;

        public HourlyForecastDisplay(HourlyForecast hour, UserSettings settings)
        {
            _hour = hour ?? throw new ArgumentNullException(nameof(hour));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        // Свойства для отображения
        public string TimeDisplay => _hour.Time.ToString("HH:mm");
        public string TemperatureDisplay => FormatTemperature(_hour.TemperatureC, _hour.TemperatureF);
        public string FeelsLikeDisplay => FormatTemperature(_hour.FeelsLikeC, _hour.FeelsLikeF);
        public string WindSpeedDisplay => FormatSpeed(_hour.WindSpeedKph, _hour.WindSpeedMph);
        public string PressureDisplay => FormatPressure(_hour.PressureMb, _hour.PressureIn);

        public int ConditionCode => _hour.ConditionCode;
        public bool IsDay => _hour.IsDay;

        public bool HasPrecipitation => _hour.PrecipitationMm > 0 || _hour.ChanceOfRain > 0;
        public string PrecipitationDisplay => _hour.PrecipitationMm > 0 ? $"{_hour.PrecipitationMm:F1} мм" : "—";
        public string ChanceOfRainDisplay => _hour.ChanceOfRain > 0 ? $"{_hour.ChanceOfRain}%" : "—";

        // Приватные методы форматирования
        private string FormatTemperature(double celsius, double fahrenheit)
        {
            return _settings.TemperatureUnit == TemperatureUnit.Celsius
                ? $"{celsius:F0}°C"
                : $"{fahrenheit:F0}°F";
        }

        private string FormatSpeed(double kph, double mph)
        {
            return _settings.SpeedUnit == SpeedUnit.KilometersPerHour
                ? $"{kph:F0} км/ч"
                : $"{mph:F0} миль/ч";
        }

        private string FormatPressure(double mb, double inhg)
        {
            return _settings.PressureUnit == PressureUnit.Millibars
                ? $"{mb:F0} мбар"
                : $"{inhg:F2} inHg";
        }
    }
}
