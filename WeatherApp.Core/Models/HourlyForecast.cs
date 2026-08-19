using System;

namespace WeatherApp.Core.Models;

/// <summary>
/// Модель почасового прогноза 
/// </summary>
public class HourlyForecast
{
    // Время
    public DateTime Time { get; set; }

    // Температура (все единицы измерения)
    public double TemperatureC { get; set; }
    public double TemperatureF { get; set; }
    public double FeelsLikeC { get; set; }
    public double FeelsLikeF { get; set; }

    // Погодные условия
    public string? ConditionText { get; set; }
    public string? ConditionIcon { get; set; }
    public int ConditionCode { get; set; }
    public bool IsDay { get; set; }

    // Детали
    public int Humidity { get; set; }
    public double WindSpeedKph { get; set; }
    public double WindSpeedMph { get; set; }
    public double PressureMb { get; set; }
    public double PressureIn { get; set; }
    public double PrecipitationMm { get; set; }
    public int CloudCover { get; set; }
    public double VisibilityKm { get; set; }

    // Вероятность осадков
    public int ChanceOfRain { get; set; }
    public int ChanceOfSnow { get; set; }
    public bool WillItRain { get; set; }
    public bool WillItSnow { get; set; }
}