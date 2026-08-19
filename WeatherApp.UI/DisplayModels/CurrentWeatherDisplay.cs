using System;
using System.Collections.Generic;
using System.Text;
using WeatherApp.Core.Models;

namespace WeatherApp.UI.DisplayModels
{
    /// Модель для отображения текущей погоды
    public class CurrentWeatherDisplay : WeatherDisplay
    {
        private readonly WeatherData _weather;

        public CurrentWeatherDisplay(WeatherData weather, UserSettings settings)
            : base(settings)
        {
            _weather = weather ?? throw new ArgumentNullException(nameof(weather));
        }
        public string CityName => _weather.CityName ?? "---";
        public string DisplayName => _weather.DisplayName;
        public string ConditionText => _weather.ConditionText ?? "---";
        public int ConditionCode => _weather.ConditionCode;
        public bool IsDay => _weather.IsDay;
        public string TemperatureDisplay => FormatTemperature(_weather.TemperatureC, _weather.TemperatureF);
        public string FeelsLikeDisplay => FormatTemperature(_weather.FeelsLikeC, _weather.FeelsLikeF);
        public float TemperatureValue => GetTemperatureValue(_weather.TemperatureC, _weather.TemperatureF);
        public string WindDirection => _weather.WindDirection ?? "---";
        public string WindSpeedDisplay => FormatSpeed(_weather.WindSpeedKph, _weather.WindSpeedMph);
        public string PressureDisplay => FormatPressure(_weather.PressureMb, _weather.PressureIn);
        public int Humidity => _weather.Humidity;
        public double UVIndex => _weather.UVIndex;
        public int ChanceOfRainToday => _weather.ChanceOfRainToday;

        public string SunriseDisplay => _weather.SunriseDisplay ?? "---";
        public string SunsetDisplay => _weather.SunsetDisplay ?? "---";
        public string PrecipitationDisplay => _weather.PrecipitationMm > 0
            ? $"{_weather.PrecipitationMm:F1} мм"
            : "---";
        public string VisibilityDisplay => _settings.SpeedUnit == SpeedUnit.KilometersPerHour
            ? $"{_weather.VisibilityKm:F1} км"
            : $"{_weather.VisibilityMiles:F1} миль";
        public string CloudCoverDisplay => $"{_weather.CloudCover}%";
    }
}