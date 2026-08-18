using System;

namespace WeatherApp.Core.Helpers;

public static class TimeHelper
{
    public static string ExtractTime(string dateTimeString)
    {
        if (string.IsNullOrEmpty(dateTimeString))
            return "--:--";

        try
        {
            if (DateTime.TryParse(dateTimeString, out var dateTime))
            {
                return dateTime.ToString("HH:mm");
            }

            var parts = dateTimeString.Split(' ');
            if (parts.Length >= 2)
            {
                var timePart = parts[1];

                if (TimeSpan.TryParse(timePart, out var timeSpan))
                {
                    return timeSpan.ToString(@"hh\:mm");
                }

                if (timePart.Contains("AM") || timePart.Contains("PM"))
                {
                    return ConvertFromAmPm(timePart);
                }
            }
        }
        catch
        {
        }

        return dateTimeString;
    }

    private static string ConvertFromAmPm(string timeStr)
    {
        try
        {
            var formats = new[] { "h:mm tt", "hh:mm tt", "h tt", "hh tt" };

            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(timeStr.Trim(), format,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out var dateTime))
                {
                    return dateTime.ToString("HH:mm");
                }
            }

            var parts = timeStr.Trim().Split(' ');
            if (parts.Length == 2)
            {
                var timePart = parts[0];
                var ampm = parts[1].ToUpperInvariant();
                var timeParts = timePart.Split(':');

                if (timeParts.Length == 2 && int.TryParse(timeParts[0], out var hour) &&
                    int.TryParse(timeParts[1], out var minute))
                {
                    if (ampm == "PM" && hour != 12)
                        hour += 12;
                    else if (ampm == "AM" && hour == 12)
                        hour = 0;

                    return $"{hour:D2}:{minute:D2}";
                }
            }
        }
        catch
        {
        }

        return timeStr;
    }

    public static string GetHourLabel(string dateTimeString)
    {
        var time = ExtractTime(dateTimeString);
        if (time.Length >= 2)
        {
            return time.Substring(0, 2) + ":00";
        }
        return time;
    }
}