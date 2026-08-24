using WeatherApp.Core.Utils;
using WeatherApp.Core.Models;

namespace WeatherApp.UI.DisplayModels;

public class ForecastDayDisplay : WeatherDisplay
{
    private readonly ForecastDay _day;

    public DateTime LocalTime { get; set; }
    public string? TimeZoneId { get; set; }

    public ForecastDayDisplay(ForecastDay day, UserSettings settings)
        : base(settings)
    {
        _day = day ?? throw new ArgumentNullException(nameof(day));

        LocalTime = day.LocalTime;
        TimeZoneId = day.TimeZoneId;
    }

    public string DayLabel => TimeZoneHelper.GetDayLabel(_day.Date, LocalTime);
    public string DayOfWeek => TimeZoneHelper.GetDayName(_day.Date);
    public string FormattedDate => _day.Date.ToString("dd MMM");

    public string MaxTempDisplay => FormatTemperature(_day.MaxTempC, _day.MaxTempF);
    public string MinTempDisplay => FormatTemperature(_day.MinTempC, _day.MinTempF);
    public string TemperatureRange => $"{MinTempDisplay} / {MaxTempDisplay}";

    public int ConditionCode => _day.ConditionCode;
    public bool IsDay => _day.Hours.FirstOrDefault()?.IsDay ?? true;

    public bool HasRain => _day.TotalPrecipMm > 0;
    public string PrecipitationDisplay => _day.TotalPrecipMm > 0 ? $"{_day.TotalPrecipMm:F1} мм" : "Без осадков";
}