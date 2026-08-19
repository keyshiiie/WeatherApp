using WeatherApp.Core.Models;

namespace WeatherApp.UI.DisplayModels;

/// <summary>
/// Базовая модель для отображения погодных данных с форматированием
/// </summary>
public class WeatherDisplay
{
    protected readonly UserSettings _settings;

    public WeatherDisplay(UserSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    // Методы форматирования
    protected string FormatTemperature(double celsius, double fahrenheit)
    {
        return _settings.TemperatureUnit == TemperatureUnit.Celsius
            ? $"{celsius:F0}°C"
            : $"{fahrenheit:F0}°F";
    }

    protected string FormatTemperatureValue(double celsius, double fahrenheit)
    {
        return _settings.TemperatureUnit == TemperatureUnit.Celsius
            ? $"{celsius:F0}"
            : $"{fahrenheit:F0}";
    }

    protected string FormatSpeed(double kph, double mph)
    {
        return _settings.SpeedUnit == SpeedUnit.KilometersPerHour
            ? $"{kph:F0} км/ч"
            : $"{mph:F0} миль/ч";
    }

    protected string FormatPressure(double mb, double inhg)
    {
        return _settings.PressureUnit == PressureUnit.Millibars
            ? $"{mb:F0} мбар"
            : $"{inhg:F2} inHg";
    }

    protected float GetTemperatureValue(double celsius, double fahrenheit)
    {
        return _settings.TemperatureUnit == TemperatureUnit.Celsius
            ? (float)celsius
            : (float)fahrenheit;
    }
}