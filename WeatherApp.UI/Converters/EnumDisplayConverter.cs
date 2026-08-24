using System.Globalization;
using WeatherApp.Core.Models;

namespace WeatherApp.UI.Converters;

public class EnumDisplayConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return string.Empty;

        var enumType = value.GetType();
        var enumName = Enum.GetName(enumType, value);

        if (string.IsNullOrEmpty(enumName)) return value.ToString() ?? string.Empty;

        var displayName = System.Text.RegularExpressions.Regex.Replace(enumName, "([a-z])([A-Z])", "$1 $2");

        return enumType.Name switch
        {
            nameof(TemperatureUnit) => displayName switch
            {
                "Celsius" => "Цельсий (°C)",
                "Fahrenheit" => "Фаренгейт (°F)",
                _ => displayName
            },
            nameof(PressureUnit) => displayName switch
            {
                "Millibars" => "Миллибары (мбар)",
                "Inches" => "Дюймы рт. ст.",
                _ => displayName
            },
            nameof(SpeedUnit) => displayName switch
            {
                "Kilometers Per Hour" => "Км/ч",
                "Miles Per Hour" => "Миль/ч",
                _ => displayName
            },
            nameof(ThemeMode) => displayName switch  
            {
                "Light" => "Светлая",
                "Dark" => "Тёмная",
                "System" => "Системная",
                _ => displayName
            },
            _ => displayName
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}