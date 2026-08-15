using System.Globalization;

namespace WeatherApp.UI.Converters;

public class WeatherBackgroundConverter : IValueConverter
{
    private static string GetBackgroundForCode(int code)
    {
        // Ясно
        if (code is 1000 or 1003 or 1006 or 1009)
            return "sunny_background.jpg";

        // Облачно
        if (code is 1012 or 1015 or 1033 or 1036 or 1039 or 1042)
            return "cloudy_background.jpg";

        // Пыльные бури
        if (code is 1018 or 1021 or 1024 or 1027 or 1045 or 1048)
            return "dust_storm_background.jpg";

        // Туман
        if (code is 1030 or 1135 or 1147)
            return "fog_background.jpg";

        // Гроза
        if (code is 1087 or (>= 1273 and <= 1282))
            return "storm_background.jpg";

        // Снег
        if (code is 1066 or (>= 1114 and <= 1117) or (>= 1210 and <= 1225) or 1255 or 1258)
            return "snow_background.jpg";

        // Мокрый снег
        //if (code is 1069 or 1204 or 1207 or 1249 or 1252)
        //    return "sleet_background.jpg";

        // Ледяные явления
        //if (code is 1168 or 1171 or 1198 or 1201 or 1237 or 1261 or 1264)
        //    return "icy_background.jpg";

        // Дождь
        if (code is 1063 or 1072 or 1150 or 1153 or
            1180 or 1183 or 1186 or 1189 or 1192 or 1195 or
            1240 or 1243 or 1246)
            return "rainy_background.jpg";

        // По умолчанию
        return "default_background.jpg";
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is int conditionCode
            ? GetBackgroundForCode(conditionCode)
            : "default_background.jpg";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}