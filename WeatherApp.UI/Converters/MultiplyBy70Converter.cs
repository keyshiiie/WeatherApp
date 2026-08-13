using System.Globalization;

namespace WeatherApp.UI.Converters;

public class MultiplyBy70Converter : IValueConverter
{
    private const float WIDTH = 70f;
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int count)
        {
            return count * WIDTH;
        }
        return 0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}