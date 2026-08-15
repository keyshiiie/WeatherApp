using System.Globalization;
using WeatherApp.Core.Models;

namespace WeatherApp.UI.Converters;

public class TemperatureDisplayConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ForecastDay day)
        {
            var unit = (TemperatureUnit)Preferences.Get("temperature_unit", (int)TemperatureUnit.Celsius);

            if (parameter?.ToString() == "max")
            {
                return unit == TemperatureUnit.Celsius
                    ? $"{day.MaxTempC:F0}°C"
                    : $"{day.MaxTempF:F0}°F";
            }
            else // min
            {
                return unit == TemperatureUnit.Celsius
                    ? $"{day.MinTempC:F0}°C"
                    : $"{day.MinTempF:F0}°F";
            }
        }
        return "--";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}