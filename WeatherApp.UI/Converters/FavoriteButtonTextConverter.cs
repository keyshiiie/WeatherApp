using System.Globalization;

namespace WeatherApp.UI.Converters;

public class FavoriteButtonTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool isFavorite && isFavorite ? "⭐ Удалить" : "☆ Добавить";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}