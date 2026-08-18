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
    public double UVIndex { get; set; }
    public double VisibilityKm { get; set; }
    public double VisibilityMiles { get; set; }
    public int CloudCover { get; set; }

    public AirQualityData? AirQuality { get; set; }

    public DateTime LastUpdated { get; set; }
    public bool IsCached { get; set; }

    public string? Region { get; set; }
    public string? Sunrise { get; set; }
    public string? Sunset { get; set; }

    public int ChanceOfRainToday { get; set; }
    public int ChanceOfSnowToday { get; set; }
    public bool WillItRainToday { get; set; }
    public bool WillItSnowToday { get; set; }

    public string SunriseDisplay => ConvertTo24HourFormat(Sunrise);
    public string SunsetDisplay => ConvertTo24HourFormat(Sunset);

    private string ConvertTo24HourFormat(string? time12h)
    {
        if (string.IsNullOrEmpty(time12h)) return "--";

        try
        {
            if (DateTime.TryParseExact(time12h, "hh:mm tt",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out DateTime time))
            {
                return time.ToString("HH:mm");
            }

            if (DateTime.TryParse(time12h, out DateTime time2))
            {
                return time2.ToString("HH:mm");
            }

            return time12h;
        }
        catch
        {
            return time12h;
        }
    }
    public string DisplayName
    {
        get
        {
            var parts = new List<string>();

            if (!string.IsNullOrEmpty(CityName))
                parts.Add(CityName);

            if (!string.IsNullOrEmpty(Region) && Region != CityName)
                parts.Add(Region);

            if (!string.IsNullOrEmpty(Country))
                parts.Add(Country);

            return parts.Count > 0 ? string.Join(", ", parts) : "Неизвестное место";
        }
    }
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