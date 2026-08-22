using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using WeatherApp.UI.DisplayModels;

namespace WeatherApp.UI.ViewModels
{
    public partial class HourlyChartViewModel : BaseViewModel, IDisposable
    {
        private List<HourlyForecastDisplay>? _allDataPoints;
        private ObservableCollection<DayGroup> _dayGroups = new();
        private DayGroup? _selectedDay;
        private int _selectedDayIndex;
        private float _minTemp;
        private float _maxTemp;
        private int _currentHourIndex = -1;
        private float _chartPadding = 0f;
        private float _pointSpacing = 100f;

        // Событие для обновления графика
        public event EventHandler<ChartDataUpdatedEventArgs>? ChartDataUpdated;

        // Коллекции и свойства
        public ObservableCollection<DayGroup> DayGroups
        {
            get => _dayGroups;
            set => SetProperty(ref _dayGroups, value);
        }

        public DayGroup? SelectedDay
        {
            get => _selectedDay;
            set
            {
                if (SetProperty(ref _selectedDay, value))
                {
                    // Обновляем IsSelected для всех групп
                    foreach (var group in DayGroups)
                    {
                        group.IsSelected = group == value;
                    }

                    if (value != null)
                    {
                        SelectedDayIndex = DayGroups.IndexOf(value);
                        UpdateChartData();
                    }
                }
            }
        }

        public int SelectedDayIndex
        {
            get => _selectedDayIndex;
            set => SetProperty(ref _selectedDayIndex, value);
        }

        public float MinTemp
        {
            get => _minTemp;
            private set => SetProperty(ref _minTemp, value);
        }

        public float MaxTemp
        {
            get => _maxTemp;
            private set => SetProperty(ref _maxTemp, value);
        }

        public int CurrentHourIndex
        {
            get => _currentHourIndex;
            private set => SetProperty(ref _currentHourIndex, value);
        }

        public float ChartPadding
        {
            get => _chartPadding;
            set => SetProperty(ref _chartPadding, value);
        }

        public float PointSpacing
        {
            get => _pointSpacing;
            set => SetProperty(ref _pointSpacing, value);
        }

        // Команды
        public ICommand SelectDayCommand { get; }
        public ICommand RefreshDataCommand { get; }

        public HourlyChartViewModel(ILogger<HourlyChartViewModel> logger)
            : base(logger)
        {
            Title = "Почасовой прогноз";

            // Инициализируем команды
            SelectDayCommand = new RelayCommand<int>(SelectDay);
            RefreshDataCommand = new RelayCommand(RefreshData);
        }

        public void SetData(List<HourlyForecastDisplay> dataPoints)
        {
            if (dataPoints == null || !dataPoints.Any())
            {
                Logger.LogWarning("SetData called with null or empty data");
                return;
            }

            Logger.LogInformation($"Setting {dataPoints.Count} data points");

            _allDataPoints = dataPoints;
            GroupDataByDay();

            if (DayGroups.Any())
            {
                SelectedDay = DayGroups.FirstOrDefault();
            }
        }

        private void GroupDataByDay()
        {
            if (_allDataPoints == null || !_allDataPoints.Any())
                return;

            Logger.LogInformation("Grouping data by day");

            DayGroups.Clear();

            var grouped = _allDataPoints
                .GroupBy(h => h.Time.Date)
                .Select((g, index) => new DayGroup
                {
                    Date = g.Key,
                    HourlyData = g.ToList(),
                    DayName = GetDayName(g.Key, index),
                    IsSelected = index == 0,
                    Index = index
                })
                .ToList();

            foreach (var group in grouped)
            {
                DayGroups.Add(group);
            }

            Logger.LogInformation($"Created {DayGroups.Count} day groups");
        }

        private string GetDayName(DateTime date, int index)
        {
            var now = DateTime.Now;

            if (date.Date == now.Date)
                return "Сегодня";
            if (date.Date == now.AddDays(1).Date)
                return "Завтра";
            if (date.Date == now.AddDays(2).Date)
                return "Послезавтра";

            // Если это первый день в списке и он не сегодня - показываем "Сегодня"
            if (index == 0 && date.Date < now.Date)
                return "Сегодня";

            var culture = new System.Globalization.CultureInfo("ru-RU");
            return culture.DateTimeFormat.GetDayName(date.DayOfWeek);
        }

        public void UpdateChartData()
        {
            if (SelectedDay?.HourlyData == null || !SelectedDay.HourlyData.Any())
            {
                Logger.LogWarning("No data for selected day");
                return;
            }

            var data = SelectedDay.HourlyData;
            Logger.LogInformation($"Updating chart with {data.Count} points for {SelectedDay.DayName}");

            // Находим индекс текущего часа
            var now = DateTime.Now;
            CurrentHourIndex = data.FindIndex(d =>
                d.Time.Hour == now.Hour && d.Time.Date == now.Date);

            if (CurrentHourIndex == -1)
                CurrentHourIndex = data.FindIndex(d => d.Time > now);

            if (CurrentHourIndex == -1 && data.Any())
                CurrentHourIndex = 0;

            // Вычисляем min/max температуры
            if (data.Any())
            {
                var temps = data.Select(d => d.TemperatureValue).ToList();
                MinTemp = temps.Min();
                MaxTemp = temps.Max();

                // Добавляем отступы для лучшего отображения
                var tempRange = MaxTemp - MinTemp;
                var padding = tempRange * 0.1f;
                if (padding < 1) padding = 1;

                MinTemp -= padding;
                MaxTemp += padding;
            }

            // Уведомляем View об обновлении
            ChartDataUpdated?.Invoke(this, new ChartDataUpdatedEventArgs
            {
                DataPoints = data,
                MinTemp = MinTemp,
                MaxTemp = MaxTemp,
                CurrentHourIndex = CurrentHourIndex,
                ChartPadding = ChartPadding,
                PointSpacing = PointSpacing
            });

            Logger.LogInformation($"Chart updated: Min={MinTemp:F1}°C, Max={MaxTemp:F1}°C, CurrentHour={CurrentHourIndex}");
        }

        private void SelectDay(int index)
        {
            if (index < 0 || index >= DayGroups.Count)
            {
                Logger.LogWarning($"Invalid day index: {index}");
                return;
            }

            var day = DayGroups[index];
            if (SelectedDay == day)
            {
                Logger.LogInformation($"Day {index} already selected");
                return;
            }

            Logger.LogInformation($"Selecting day {index}: {day.DayName}");
            SelectedDay = day;
        }

        private void RefreshData()
        {
            Logger.LogInformation("Refreshing chart data");
            if (_allDataPoints != null)
            {
                SetData(_allDataPoints);
            }
        }

        // Метод для обновления всех данных
        public void UpdateAllData(List<HourlyForecastDisplay> dataPoints)
        {
            SetData(dataPoints);
        }

        // Получить данные для текущего дня
        public List<HourlyForecastDisplay>? GetCurrentDayData()
        {
            return SelectedDay?.HourlyData;
        }

        // Проверить, есть ли данные
        public bool HasData => DayGroups.Any() && DayGroups.Any(g => g.HourlyData?.Any() == true);

        public void Dispose()
        {
            ChartDataUpdated = null;
            GC.SuppressFinalize(this);
        }
    }

}
