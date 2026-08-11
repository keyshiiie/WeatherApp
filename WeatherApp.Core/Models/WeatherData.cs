using System;

namespace WeatherApp.Core.Models;

/// <summary>
/// Модель текущей погоды для отображения в UI
/// </summary>
public class WeatherData
{
    public string? CityName { get; set; }
    public string? Country { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }

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
    public string? WindDirection { get; set; }
    public double PressureMb { get; set; }
    public double PressureIn { get; set; }
    public double PrecipitationMm { get; set; }
    public double PrecipitationIn { get; set; }
    public int UVIndex { get; set; }
    public double VisibilityKm { get; set; }
    public double VisibilityMiles { get; set; }
    public int CloudCover { get; set; }

    public AirQualityData? AirQuality { get; set; }

    public DateTime LastUpdated { get; set; }
    public bool IsCached { get; set; }
}
public class AirQualityData
{
    public double Co { get; set; }
    public double No2 { get; set; }
    public double O3 { get; set; }
    public double So2 { get; set; }
    public double Pm25 { get; set; }
    public double Pm10 { get; set; }
    public int UsEpaIndex { get; set; }
    public int GbDefraIndex { get; set; }
}