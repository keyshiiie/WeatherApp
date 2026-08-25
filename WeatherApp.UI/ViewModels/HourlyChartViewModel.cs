using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using WeatherApp.Core.Utils;
using WeatherApp.UI.DisplayModels;
using WeatherApp.UI.Services; // Добавляем

namespace WeatherApp.UI.ViewModels;

public partial class HourlyChartViewModel : BaseViewModel, IDisposable
{
    [ObservableProperty]
    public partial ObservableCollection<DayGroup> DayGroups { get; set; } = new();

    [ObservableProperty]
    public partial DayGroup? SelectedDay { get; set; }
    [ObservableProperty]
    public partial List<HourlyForecastDisplay>? CurrentDayData { get; set; }

    [ObservableProperty]
    public partial float MinTemp { get; set; }
    [ObservableProperty]
    public partial float MaxTemp { get; set; }
    [ObservableProperty]
    public partial int CurrentHourIndex { get; set; } = -1;

    [ObservableProperty]
    public partial bool HasData { get; set; }
    [ObservableProperty]
    public partial string EmptyStateMessage { get; set; } = "Нет данных";

    public event EventHandler<ChartDataUpdatedEventArgs>? ChartDataUpdated;

    public HourlyChartViewModel(
        ILogger<HourlyChartViewModel> logger,
        INavigationService navigationService) 
        : base(logger, navigationService)
    {
        Title = "Почасовой прогноз";
    }

    public void SetData(List<HourlyForecastDisplay> dataPoints)
    {
        if (dataPoints == null || dataPoints.Count == 0)
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

        if (dataPoints == null || dataPoints.Count == 0)
            return;

        var localTime = dataPoints.First().LocalTime;

        var grouped = dataPoints
            .GroupBy(h => h.Time.Date)
            .Select((g, index) => new DayGroup
            {
                Date = g.Key,
                HourlyData = g.ToList(),
                DayName = TimeZoneHelper.GetDayLabel(g.Key, localTime),
                IsSelected = index == 0,
                Index = index
            })
            .ToList();

        foreach (var group in grouped)
        {
            DayGroups.Add(group);
        }
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

        var localTime = CurrentDayData.First().LocalTime;
        var currentHour = localTime.Hour;
        var currentDate = localTime.Date;

        CurrentHourIndex = CurrentDayData.FindIndex(d =>
            d.Time.Hour == currentHour && d.Time.Date == currentDate);

        if (CurrentHourIndex == -1)
            CurrentHourIndex = CurrentDayData.FindIndex(d => d.Time > localTime);

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