using System;

namespace WeatherApp.Core.Models;
public class HourlyForecast
{
    public DateTime Time { get; set; }
    public string TimeDisplay => Time.ToString("HH:mm");

    // Температура
    public double TemperatureC { get; set; }
    public double TemperatureF { get; set; }
    public double FeelsLikeC { get; set; }
    public double FeelsLikeF { get; set; }

    // Состояние погоды
    public string? ConditionText { get; set; }
    public string? ConditionIcon { get; set; }
    public int ConditionCode { get; set; }
    public bool IsDay { get; set; }

    // Детали
    public int Humidity { get; set; }
    public double WindSpeedKph { get; set; }
    public double WindSpeedMph { get; set; }
    public double PressureMb { get; set; }
    public double PrecipitationMm { get; set; }
    public int CloudCover { get; set; }
    public double VisibilityKm { get; set; }

    // Вероятность осадков
    public int ChanceOfRain { get; set; }
    public int ChanceOfSnow { get; set; }
    public bool WillItRain { get; set; }
    public bool WillItSnow { get; set; }

    public bool HasPrecipitation => PrecipitationMm > 0 || ChanceOfRain > 0;
}