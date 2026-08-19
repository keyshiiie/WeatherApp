using WeatherApp.Core.Models;

namespace WeatherApp.UI.ViewModels;

/// <summary>
/// Модель для отображения дня прогноза
/// </summary>
public class ForecastDayDisplay
{
    private readonly ForecastDay _day;
    private readonly UserSettings _settings;

    public ForecastDayDisplay(ForecastDay day, UserSettings settings)
    {
        _day = day ?? throw new ArgumentNullException(nameof(day));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    // Свойства для отображения
    public string DayLabel => GetDayLabel();
    public string DayOfWeek => _day.Date.ToString("ddd");
    public string FormattedDate => _day.Date.ToString("dd MMM");

    public string MaxTempDisplay => FormatTemperature(_day.MaxTempC, _day.MaxTempF);
    public string MinTempDisplay => FormatTemperature(_day.MinTempC, _day.MinTempF);
    public string TemperatureRange => $"{MinTempDisplay} / {MaxTempDisplay}";

    public int ConditionCode => _day.ConditionCode;
    public bool IsDay => _day.Hours.FirstOrDefault()?.IsDay ?? true;

    public bool HasRain => _day.TotalPrecipMm > 0;
    public string PrecipitationDisplay => _day.TotalPrecipMm > 0 ? $"{_day.TotalPrecipMm:F1} мм" : "Без осадков";

    // Приватные методы форматирования
    private string FormatTemperature(double celsius, double fahrenheit)
    {
        return _settings.TemperatureUnit == TemperatureUnit.Celsius
            ? $"{celsius:F0}°C"
            : $"{fahrenheit:F0}°F";
    }

    private string GetDayLabel()
    {
        var today = DateTime.Today;
        var day = _day.Date.Date;

        if (day == today)
            return "Сегодня";

        if (day == today.AddDays(1))
            return "Завтра";

        return _day.Date.ToString("ddd");
    }
}