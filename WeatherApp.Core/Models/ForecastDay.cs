using System;
using System.Collections.Generic;

namespace WeatherApp.Core.Models;

/// <summary>
/// Модель дня прогноза для отображения в UI
/// </summary>
public class ForecastDay
{
    public DateTime Date { get; set; }
    public string DayOfWeek => Date.ToString("ddd");
    public string FormattedDate => Date.ToString("dd MMM");

    // Температура
    public double MaxTempC { get; set; }
    public double MinTempC { get; set; }
    public double AvgTempC { get; set; }
    public double MaxTempF { get; set; }
    public double MinTempF { get; set; }
    public double AvgTempF { get; set; }

    // Состояние погоды
    public string? ConditionText { get; set; }
    public string? ConditionIcon { get; set; }
    public int ConditionCode { get; set; }

    // Детали
    public double MaxWindKph { get; set; }
    public double TotalPrecipMm { get; set; }
    public int AvgHumidity { get; set; }
    public double UVIndex { get; set; }
    public double AvgVisibilityKm { get; set; }

    // Астрономические данные
    public string? Sunrise { get; set; }
    public string? Sunset { get; set; }
    public string? Moonrise { get; set; }
    public string? Moonset { get; set; }
    public string? MoonPhase { get; set; }
    public int MoonIllumination { get; set; } 

    // Почасовой прогноз
    public List<HourlyForecast> Hours { get; set; } = new();

    public string TemperatureRange => $"{MinTempC}° / {MaxTempC}°";
    public bool HasRain => TotalPrecipMm > 0;

    // Для отображения в UI
    public string MoonIlluminationDisplay => $"{MoonIllumination}%";
}