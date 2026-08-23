using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using WeatherApp.UI.DisplayModels;

namespace WeatherApp.UI.ViewModels;

public partial class HourlyChartViewModel : BaseViewModel, IDisposable
{
    [ObservableProperty]
    private ObservableCollection<DayGroup> _dayGroups = new();

    [ObservableProperty]
    private DayGroup? _selectedDay;

    [ObservableProperty]
    private List<HourlyForecastDisplay>? _currentDayData;

    [ObservableProperty]
    private float _minTemp;

    [ObservableProperty]
    private float _maxTemp;

    [ObservableProperty]
    private int _currentHourIndex = -1;

    [ObservableProperty]
    private bool _hasData;

    [ObservableProperty]
    private string _emptyStateMessage = "Нет данных";

    public event EventHandler<ChartDataUpdatedEventArgs>? ChartDataUpdated;

    public HourlyChartViewModel(ILogger<HourlyChartViewModel> logger) : base(logger)
    {
        Title = "Почасовой прогноз";
    }

    public void SetData(List<HourlyForecastDisplay> dataPoints)
    {
        if (dataPoints == null || !dataPoints.Any())
        {
            HasData = false;
            EmptyStateMessage = "Нет данных для отображения";
            CurrentDayData = null;
            DayGroups.Clear();
            return;
        }

        HasData = true;
        GroupDataByDay(dataPoints);

        if (DayGroups.Any())
        {
            SelectedDay = DayGroups.First();
            UpdateCurrentDayData();
            RaiseChartDataUpdated();
        }
    }

    private void GroupDataByDay(List<HourlyForecastDisplay> dataPoints)
    {
        DayGroups.Clear();

        var now = DateTime.Now;
        var grouped = dataPoints
            .GroupBy(h => h.Time.Date)
            .Select((g, index) => new DayGroup
            {
                Date = g.Key,
                HourlyData = g.ToList(),
                DayName = GetDayName(g.Key, now, index),
                IsSelected = index == 0,
                Index = index
            })
            .ToList();

        foreach (var group in grouped)
        {
            DayGroups.Add(group);
        }
    }

    private string GetDayName(DateTime date, DateTime now, int index)
    {
        if (date.Date == now.Date)
            return "Сегодня";
        if (date.Date == now.AddDays(1).Date)
            return "Завтра";
        if (date.Date == now.AddDays(2).Date)
            return "Послезавтра";

        var culture = new System.Globalization.CultureInfo("ru-RU");
        return culture.DateTimeFormat.GetDayName(date.DayOfWeek);
    }

    [RelayCommand]
    private void SelectDay(DayGroup day)
    {
        if (day == null || SelectedDay == day)
            return;

        foreach (var group in DayGroups)
        {
            group.IsSelected = group == day;
        }

        SelectedDay = day;
        UpdateCurrentDayData();
        RaiseChartDataUpdated();
    }

    private void UpdateCurrentDayData()
    {
        if (SelectedDay?.HourlyData == null || !SelectedDay.HourlyData.Any())
        {
            CurrentDayData = null;
            return;
        }

        CurrentDayData = SelectedDay.HourlyData;

        var now = DateTime.Now;
        CurrentHourIndex = CurrentDayData.FindIndex(d =>
            d.Time.Hour == now.Hour && d.Time.Date == now.Date);

        if (CurrentHourIndex == -1)
            CurrentHourIndex = CurrentDayData.FindIndex(d => d.Time > now);

        if (CurrentHourIndex == -1 && CurrentDayData.Any())
            CurrentHourIndex = 0;

        var temps = CurrentDayData.Select(d => d.TemperatureValue).ToList();
        MinTemp = temps.Min();
        MaxTemp = temps.Max();

        var padding = (MaxTemp - MinTemp) * 0.1f;
        if (padding < 1) padding = 1;
        MinTemp -= padding;
        MaxTemp += padding;
    }

    private void RaiseChartDataUpdated()
    {
        if (CurrentDayData == null || !CurrentDayData.Any())
            return;

        ChartDataUpdated?.Invoke(this, new ChartDataUpdatedEventArgs
        {
            DataPoints = CurrentDayData,
            MinTemp = MinTemp,
            MaxTemp = MaxTemp,
            CurrentHourIndex = CurrentHourIndex,
            ChartPadding = 0f,
            PointSpacing = 100f
        });
    }

    public void Dispose()
    {
        ChartDataUpdated = null;
        DayGroups.Clear();
        CurrentDayData = null;
        GC.SuppressFinalize(this);
    }
}