using System;
using WeatherApp.Core.Models;

namespace WeatherApp.UI.ViewModels
{
    /// <summary>
    /// Модель для отображения текущей погоды
    /// </summary>
    public class WeatherDataDisplay
    {
        private readonly WeatherData _weather;
        private readonly UserSettings _settings;

        public WeatherDataDisplay(WeatherData weather, UserSettings settings)
        {
            _weather = weather ?? throw new ArgumentNullException(nameof(weather));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        // Делегируем доступ к свойствам
        public string CityName => _weather.CityName ?? "--";
        public string Country => _weather.Country ?? "--";
        public string Region => _weather.Region ?? "--";
        public string DisplayName => _weather.DisplayName;
        public double Latitude => _weather.Latitude;
        public double Longitude => _weather.Longitude;
        public int ConditionCode => _weather.ConditionCode;
        public bool IsDay => _weather.IsDay;
        public string ConditionText => _weather.ConditionText ?? "--";
        public int Humidity => _weather.Humidity;
        public string WindDirection => _weather.WindDirection ?? "--";
        public double UVIndex => _weather.UVIndex;
        public int ChanceOfRainToday => _weather.ChanceOfRainToday;
        public string SunriseDisplay => _weather.SunriseDisplay;
        public string SunsetDisplay => _weather.SunsetDisplay;

        // Форматированные значения
        public string TemperatureDisplay => FormatTemperature(_weather.TemperatureC, _weather.TemperatureF);
        public string FeelsLikeDisplay => FormatTemperature(_weather.FeelsLikeC, _weather.FeelsLikeF);
        public string PressureDisplay => FormatPressure(_weather.PressureMb, _weather.PressureIn);
        public string WindSpeedDisplay => FormatSpeed(_weather.WindSpeedKph, _weather.WindSpeedMph);

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