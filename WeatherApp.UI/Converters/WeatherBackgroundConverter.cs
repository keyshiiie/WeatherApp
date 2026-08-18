using System.Globalization;
using Microsoft.Maui.Graphics;

namespace WeatherApp.UI.Converters;

public class WeatherBackgroundConverter : IValueConverter
{
    private static readonly Color DayColor = Color.FromArgb("#4D70F1");
    private static readonly Color NightColor = Color.FromArgb("#010C38");

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Ожидаем, что value это bool (IsDay) или int (1 - день, 0 - ночь)
        bool isDay = true;

        if (value is bool isDayBool)
        {
            isDay = isDayBool;
        }
        else if (value is int isDayInt)
        {
            isDay = isDayInt == 1;
        }
        else if (value is string isDayStr && bool.TryParse(isDayStr, out var parsedDay))
        {
            isDay = parsedDay;
        }

        var color = isDay ? DayColor : NightColor;

        // Если параметр "String" - возвращаем строку в формате hex
        if (parameter?.ToString() == "String")
            return color.ToHex();

        // Иначе возвращаем Color
        return color;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}