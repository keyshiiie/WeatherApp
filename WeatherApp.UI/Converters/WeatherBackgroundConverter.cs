using System.Globalization;
using Microsoft.Maui.Graphics;

namespace WeatherApp.UI.Converters;

public class WeatherBackgroundConverter : IValueConverter
{
    private static Color GetColorForCode(int code)
    {
        // Ясно
        if (code is 1000 or 1003 or 1006 or 1009)
            return Colors.LightSkyBlue;

        // Облачно
        if (code is 1012 or 1015 or 1033 or 1036 or 1039 or 1042)
            return Colors.Gray;

        // Пыльные бури
        if (code is 1018 or 1021 or 1024 or 1027 or 1045 or 1048)
            return Colors.SandyBrown;

        // Туман
        if (code is 1030 or 1135 or 1147)
            return Colors.LightGray;

        // Гроза
        if (code is 1087 or (>= 1273 and <= 1282))
            return Colors.DarkSlateBlue;

        // Снег
        if (code is 1066 or (>= 1114 and <= 1117) or (>= 1210 and <= 1225) or 1255 or 1258)
            return Colors.WhiteSmoke;

        //Мокрый снег(если раскомментировать)
        if (code is 1069 or 1204 or 1207 or 1249 or 1252)
            return Colors.LightSteelBlue;

        //Ледяные явления(если раскомментировать)
        if (code is 1168 or 1171 or 1198 or 1201 or 1237 or 1261 or 1264)
            return Colors.LightCyan;

        // Дождь
        if (code is 1063 or 1072 or 1150 or 1153 or
            1180 or 1183 or 1186 or 1189 or 1192 or 1195 or
            1240 or 1243 or 1246)
            return Colors.DarkBlue;

        // По умолчанию
        return Colors.White;
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int conditionCode)
        {
            var color = GetColorForCode(conditionCode);

            // Если параметр "String" - возвращаем строку в формате hex
            if (parameter?.ToString() == "String")
                return color.ToHex();

            // Иначе возвращаем Color
            return color;
        }

        return parameter?.ToString() == "String"
            ? Colors.White.ToHex()
            : Colors.White;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}