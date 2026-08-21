using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using WeatherApp.Core.Models;
using WeatherApp.UI.DisplayModels;

namespace WeatherApp.UI.Views;

public partial class HourlyChartView : ContentView
{
    private List<HourlyForecastDisplay>? _allDataPoints;
    private List<List<HourlyForecastDisplay>>? _groupedByDay;
    private List<HourlyForecastDisplay>? _dataPoints;
    private int _selectedDayIndex = 0;
    private float _minTemp;
    private float _maxTemp;
    private ChartDrawable? _chartDrawable;
    private int _currentHourIndex = -1;
    private bool _isInitialized = false;
    private bool _isFirstLoad = true;

    // Цвет для текущего часа - только точка на графике
    private static readonly Color CurrentHourColor = Color.FromArgb("#FFC24B");
    private static readonly Color TabSelectedColor = Color.FromArgb("#FFC24B");
    private static readonly Color TabUnselectedColor = Colors.White;

    // Настройки
    private const float ChartPadding = 0f;
    private const float PointSpacing = 100f;
    private const float DataBlockWidth = PointSpacing;

    public HourlyChartView()
    {
        InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("✅ HourlyChartView: Loaded event fired");
        _isInitialized = true;

        _chartDrawable = new ChartDrawable();
        if (ChartGraphicsView != null)
        {
            ChartGraphicsView.Drawable = _chartDrawable;
        }

        // Если данные уже были установлены, обновляем график
        if (_dataPoints != null && _dataPoints.Any())
        {
            System.Diagnostics.Debug.WriteLine($"✅ HourlyChartView: Redrawing on Loaded with {_dataPoints.Count} points");
            // Принудительно обновляем график
            _isFirstLoad = false;
            UpdateChart();
            Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(200), ScrollToCurrentHour);
        }
    }

    public void SetData(List<HourlyForecastDisplay> dataPoints)
    {
        if (dataPoints == null || !dataPoints.Any())
        {
            System.Diagnostics.Debug.WriteLine("❌ HourlyChartView: No data points");
            return;
        }

        System.Diagnostics.Debug.WriteLine($"✅ HourlyChartView: Received {dataPoints.Count} data points");

        _allDataPoints = dataPoints;
        _dataPoints = dataPoints;
        GroupDataByDay();
        CreateDayTabs();

        // Если View уже инициализирован, сразу обновляем график
        if (_isInitialized)
        {
            System.Diagnostics.Debug.WriteLine("✅ HourlyChartView: Already initialized, updating chart");
            _isFirstLoad = false;
            // Принудительно выбираем день и обновляем график
            _selectedDayIndex = 0;
            _dataPoints = _groupedByDay?[0];
            UpdateTabs();
            UpdateChart();
            Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(200), ScrollToCurrentHour);
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("✅ HourlyChartView: Waiting for Loaded event");
            _selectedDayIndex = 0;
            _isFirstLoad = false;
        }
    }

    private void GroupDataByDay()
    {
        if (_allDataPoints == null || !_allDataPoints.Any())
        {
            _groupedByDay = new List<List<HourlyForecastDisplay>>();
            return;
        }

        _groupedByDay = new List<List<HourlyForecastDisplay>>();
        var currentDay = _allDataPoints.First().Time.Date;
        var currentGroup = new List<HourlyForecastDisplay>();

        foreach (var hour in _allDataPoints)
        {
            if (hour.Time.Date != currentDay)
            {
                if (currentGroup.Any())
                {
                    _groupedByDay.Add(currentGroup);
                }
                currentDay = hour.Time.Date;
                currentGroup = new List<HourlyForecastDisplay>();
            }
            currentGroup.Add(hour);
        }

        if (currentGroup.Any())
        {
            _groupedByDay.Add(currentGroup);
        }

        System.Diagnostics.Debug.WriteLine($"✅ Grouped into {_groupedByDay.Count} days");
    }

    private void CreateDayTabs()
    {
        if (DayTabs == null || _groupedByDay == null || !_groupedByDay.Any())
            return;

        DayTabs.Children.Clear();

        var now = DateTime.Now;

        for (int i = 0; i < _groupedByDay.Count; i++)
        {
            var dayData = _groupedByDay[i];
            var dayDate = dayData.First().Time.Date;
            var dayName = GetDayName(dayDate, now);

            var index = i;

            var tabButton = new Button
            {
                Text = dayName,
                FontFamily = index == _selectedDayIndex ? "InterBold" : "InterMedium",
                FontSize = 14,
                BackgroundColor = Colors.Transparent,
                TextColor = index == _selectedDayIndex ? TabSelectedColor : TabUnselectedColor,
                Padding = new Thickness(12, 6),
                CornerRadius = 20,
                BorderWidth = 0,
                ClassId = index.ToString(),
                Command = new Command(() =>
                {
                    System.Diagnostics.Debug.WriteLine($"🔄 Tab clicked: {index} - {dayName}");
                    SelectDay(index);
                })
            };

            DayTabs.Children.Add(tabButton);
        }

        UpdateTabs();
    }

    private string GetDayName(DateTime date, DateTime now)
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

    private void SelectDay(int index)
    {
        if (_groupedByDay == null || index < 0 || index >= _groupedByDay.Count)
            return;

        if (_selectedDayIndex == index && _dataPoints != null && _dataPoints.Any() && !_isFirstLoad)
        {
            System.Diagnostics.Debug.WriteLine($"ℹ️ Day {index} already selected");
            return;
        }

        System.Diagnostics.Debug.WriteLine($"🔄 Selecting day {index}");
        _selectedDayIndex = index;
        _dataPoints = _groupedByDay[index];
        _isFirstLoad = false;
        UpdateTabs();
        UpdateChart();

        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(200), ScrollToCurrentHour);
    }

    private void UpdateTabs()
    {
        if (DayTabs == null)
            return;

        for (int i = 0; i < DayTabs.Children.Count; i++)
        {
            var tab = DayTabs.Children[i] as Button;
            if (tab == null) continue;

            var isSelected = (i == _selectedDayIndex);
            tab.TextColor = isSelected ? TabSelectedColor : TabUnselectedColor;
            tab.FontFamily = isSelected ? "InterBold" : "InterMedium";
        }
    }

    private void UpdateChart()
    {
        if (_dataPoints == null || !_dataPoints.Any())
        {
            System.Diagnostics.Debug.WriteLine("❌ UpdateChart: No data points");
            return;
        }

        System.Diagnostics.Debug.WriteLine($"✅ UpdateChart: Updating with {_dataPoints.Count} points");

        // Находим индекс текущего часа
        var now = DateTime.Now;
        _currentHourIndex = _dataPoints.FindIndex(d =>
            d.Time.Hour == now.Hour && d.Time.Date == now.Date);

        if (_currentHourIndex == -1)
        {
            _currentHourIndex = _dataPoints.FindIndex(d => d.Time > now);
        }

        if (_currentHourIndex == -1 && _dataPoints.Any())
        {
            _currentHourIndex = 0;
        }

        System.Diagnostics.Debug.WriteLine($"✅ Current hour index: {_currentHourIndex}");

        var temps = _dataPoints.Select(d => d.TemperatureValue).ToList();
        _minTemp = temps.Min();
        _maxTemp = temps.Max();

        var padding = (_maxTemp - _minTemp) * 0.1f;
        if (padding < 1) padding = 1;
        _minTemp -= padding;
        _maxTemp += padding;

        var totalWidth = ChartPadding + (_dataPoints.Count - 1) * PointSpacing + PointSpacing;

        if (ChartGraphicsView != null)
        {
            ChartGraphicsView.WidthRequest = totalWidth;
            ChartGraphicsView.HeightRequest = 180;
        }

        if (_chartDrawable == null)
            _chartDrawable = new ChartDrawable();

        _chartDrawable.SetData(_dataPoints, _minTemp, _maxTemp, ChartPadding, PointSpacing, _currentHourIndex);

        if (ChartGraphicsView != null)
        {
            ChartGraphicsView.Drawable = _chartDrawable;
            ChartGraphicsView.Invalidate();
        }

        UpdateDataList();
    }

    private void UpdateDataList()
    {
        if (DataStack == null || _dataPoints == null)
            return;

        DataStack.Children.Clear();

        if (_dataPoints.Any())
        {
            var spacer = new BoxView
            {
                WidthRequest = ChartPadding,
                HeightRequest = 1,
                Color = Colors.Transparent
            };
            DataStack.Children.Add(spacer);
        }

        for (int i = 0; i < _dataPoints.Count; i++)
        {
            var hour = _dataPoints[i];
            var isCurrentHour = (i == _currentHourIndex);

            var container = new StackLayout
            {
                Spacing = 4,
                HorizontalOptions = LayoutOptions.Center,
                WidthRequest = DataBlockWidth,
                Padding = new Thickness(0, 5)
            };

            // Время
            var timeLabel = new Label
            {
                Text = hour.TimeDisplay,
                TextColor = Colors.White,
                FontFamily = isCurrentHour ? "InterBold" : "InterMedium",
                FontSize = isCurrentHour ? 13 : 12,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center
            };
            container.Children.Add(timeLabel);

            // Иконка погоды
            var icon = new Image
            {
                WidthRequest = 30,
                HeightRequest = 30,
                HorizontalOptions = LayoutOptions.Center
            };

            try
            {
                var multiBinding = new MultiBinding
                {
                    Converter = Application.Current.Resources["WeatherIconMulti"] as IMultiValueConverter
                };

                multiBinding.Bindings.Add(new Binding
                {
                    Source = hour,
                    Path = "ConditionCode"
                });
                multiBinding.Bindings.Add(new Binding
                {
                    Source = hour,
                    Path = "IsDay"
                });

                icon.SetBinding(Image.SourceProperty, multiBinding);
            }
            catch
            {
                icon.Source = "appic_sun.png";
            }

            container.Children.Add(icon);

            // Температура - всегда белая
            var tempLabel = new Label
            {
                Text = hour.TemperatureDisplay,
                TextColor = Colors.White,
                FontFamily = "InterSemiBold",
                FontSize = 16,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center
            };
            container.Children.Add(tempLabel);

            // Ветер
            var windLabel = new Label
            {
                Text = hour.WindSpeedDisplay,
                TextColor = Colors.White.WithAlpha(0.7f),
                FontFamily = "InterMedium",
                FontSize = 10,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center
            };
            container.Children.Add(windLabel);

            DataStack.Children.Add(container);
        }

        if (_dataPoints.Any())
        {
            var spacerRight = new BoxView
            {
                WidthRequest = ChartPadding,
                HeightRequest = 1,
                Color = Colors.Transparent
            };
            DataStack.Children.Add(spacerRight);
        }
    }

    private void ScrollToCurrentHour()
    {
        if (_currentHourIndex < 0 || MainScrollView == null)
            return;

        try
        {
            var screenWidth = this.Width;
            if (screenWidth <= 0) return;

            var targetX = _currentHourIndex * PointSpacing - screenWidth / 2 + PointSpacing / 2;
            if (targetX < 0) targetX = 0;

            MainScrollView.ScrollToAsync(targetX, 0, true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Scroll error: {ex.Message}");
        }
    }

    public void RefreshData()
    {
        if (_dataPoints != null)
        {
            UpdateChart();
        }
    }

    public void UpdateAllData(List<HourlyForecastDisplay> dataPoints)
    {
        SetData(dataPoints);
    }
}