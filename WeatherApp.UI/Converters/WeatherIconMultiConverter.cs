using System.Globalization;
using Microsoft.Maui.Controls;

namespace WeatherApp.UI.Converters;

public class WeatherIconMultiConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        // Проверяем, что пришло 2 значения: ConditionCode и IsDay
        if (values.Length < 2 || values[0] is not int conditionCode)
            return "ic_unknown.png";

        // Определяем день/ночь
        bool isDay = true;

        if (values[1] is bool isDayBool)
        {
            isDay = isDayBool;
        }
        else if (values[1] is int isDayInt)
        {
            isDay = isDayInt == 1;
        }
        else if (values[1] is string isDayStr && bool.TryParse(isDayStr, out var parsedDay))
        {
            isDay = parsedDay;
        }

        return GetIconForCode(conditionCode, isDay);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private static string GetIconForCode(int code, bool isDay)
    {
        // Ясно 
        if (code == 1000)
            return isDay ? "ic_sunny.png" : "ic_clear_night.png";

        // Малооблачно / Переменная облачность 
        if (code == 1003)
            return isDay ? "ic_partly_cloudy.png" : "ic_partly_cloudy_night.png";

        // Облачно
        if (code is 1006 or 1009)
            return "ic_cloudy.png"; // Облачно днем и ночью одинаково

        // Туман
        if (code is 1030 or 1135 or 1147)
            return isDay ? "ic_fog.png" : "ic_fog_night.png";

        // Морось / Мелкий дождь
        if (code is 1063 or 1072 or 1150 or 1153 or 1168 or 1171 or 1180 or 1183 or 1240)
            return isDay ? "ic_drizzle.png" : "ic_drizzle_night.png";

        // Умеренный дождь
        if (code is 1186 or 1189 or 1243)
            return "ic_rain_moderate.png"; // Днем и ночью одинаково

        // Сильный дождь
        if (code is 1192 or 1195 or 1246)
            return "ic_rain_heavy.png"; // Днем и ночью одинаково

        // Замерзающий дождь
        if (code is 1198 or 1201)
            return "ic_freezing_rain.png"; // Днем и ночью одинаково

        // Снег
        if (code is 1066 or 1210 or 1213 or 1216 or 1219 or 1222 or 1225 or 1255 or 1258)
            return isDay ? "ic_snow.png" : "ic_snow_night.png";

        // Снег с дождём / Мокрый снег
        if (code is 1069 or 1204 or 1207 or 1249 or 1252)
            return isDay ? "ic_sleet.png" : "ic_sleet_night.png";

        // Ледяные явления
        if (code is 1237 or 1261 or 1264)
            return isDay ? "ic_ice_pellets.png" : "ic_ice_pellets_night.png";

        // Гроза
        if (code is 1087 or 1273 or 1276 or 1279 or 1282)
            return isDay ? "ic_thunder.png" : "ic_thunder_night.png";

        // Пыльные бури / Песчаные бури
        if (code is 1018 or 1021 or 1024 or 1027 or 1045 or 1048)
            return "ic_dust.png"; // Днем и ночью одинаково

        // Ветер
        if (code is 1114 or 1117)
            return isDay ? "ic_windy.png" : "ic_windy_night.png";

        // По умолчанию
        return isDay ? "ic_unknown.png" : "ic_unknown_night.png";
    }
}