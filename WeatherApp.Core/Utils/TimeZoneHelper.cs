namespace WeatherApp.Core.Utils;

public static class TimeZoneHelper
{
    public static DateTime GetLocalDate(DateTime localTime)
    {
        return localTime.Date;
    }

    public static bool IsToday(DateTime date, DateTime localTime)
    {
        return date.Date == localTime.Date;
    }

    public static bool IsTomorrow(DateTime date, DateTime localTime)
    {
        return date.Date == localTime.Date.AddDays(1);
    }

    public static bool IsDayAfterTomorrow(DateTime date, DateTime localTime)
    {
        return date.Date == localTime.Date.AddDays(2);
    }

    public static string GetDayName(DateTime date)
    {
        var culture = new System.Globalization.CultureInfo("ru-RU");
        var dayName = culture.DateTimeFormat.GetDayName(date.DayOfWeek);
        return char.ToUpper(dayName[0]) + dayName.Substring(1);
    }
    public static string GetDayLabel(DateTime date, DateTime localTime)
    {
        if (IsToday(date, localTime))
            return "Сегодня";

        if (IsTomorrow(date, localTime))
            return "Завтра";

        if (IsDayAfterTomorrow(date, localTime))
            return "Послезавтра";

        return GetDayName(date);
    }
}