using System;
using WeatherApp.Core.Models;

namespace WeatherApp.UI.DisplayModels
{
    /// <summary>
    /// Модель для отображения почасового прогноза
    /// </summary>
    public class HourlyForecastDisplay : WeatherDisplay
    {
        private readonly HourlyForecast _hour;

        public HourlyForecastDisplay(HourlyForecast hour, UserSettings settings)
            : base(settings)
        {
            _hour = hour ?? throw new ArgumentNullException(nameof(hour));
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

        // Свойство для графика
        public float TemperatureValue => GetTemperatureValue(_hour.TemperatureC, _hour.TemperatureF);
    }
}