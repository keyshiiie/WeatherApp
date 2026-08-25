using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using WeatherApp.UI.DisplayModels;
using WeatherApp.UI.Services;
using WeatherApp.UI.ViewModels;

namespace WeatherApp.UI.Views;

public partial class HourlyChartView : ContentView, IDisposable
{
    private HourlyChartViewModel? _viewModel;
    private ChartDrawable? _chartDrawable;
    private bool _isDisposed;

    public static readonly BindableProperty DataPointsProperty =
        BindableProperty.Create(
            nameof(DataPoints),
            typeof(List<HourlyForecastDisplay>),
            typeof(HourlyChartView),
            propertyChanged: OnDataPointsChanged);

    public List<HourlyForecastDisplay>? DataPoints
    {
        get => (List<HourlyForecastDisplay>?)GetValue(DataPointsProperty);
        set => SetValue(DataPointsProperty, value);
    }

    // Конструктор без параметров для XAML
    public HourlyChartView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        SizeChanged += OnSizeChanged;
    }

    // Метод для установки ViewModel через DI
    public void SetViewModel(HourlyChartViewModel viewModel)
    {
        if (_viewModel != null)
        {
            _viewModel.ChartDataUpdated -= OnChartDataUpdated;
        }

        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        BindingContext = _viewModel;

        _viewModel.ChartDataUpdated += OnChartDataUpdated;

        // Если уже загружены данные, обновляем
        if (_viewModel.HasData)
        {
            UpdateChart();
        }
    }

    public void SetData(List<HourlyForecastDisplay> dataPoints)
    {
        _viewModel?.SetData(dataPoints);
    }

    private static void OnDataPointsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (HourlyChartView)bindable;
        var data = (List<HourlyForecastDisplay>?)newValue;

        if (data != null && data.Count != 0)
        {
            view.SetData(data);
        }
    }

    private void OnSizeChanged(object? sender, EventArgs e)
    {
        if (_isDisposed || _viewModel == null || !_viewModel.HasData || _chartDrawable == null)
            return;

        Dispatcher.Dispatch(() =>
        {
            if (_viewModel.CurrentDayData != null)
            {
                _chartDrawable.SetData(
                    _viewModel.CurrentDayData,
                    _viewModel.MinTemp,
                    _viewModel.MaxTemp,
                    0f,
                    100f,
                    _viewModel.CurrentHourIndex);

                ChartGraphicsView.Invalidate();
            }
        });
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        if (_isDisposed) return;

        _chartDrawable = new ChartDrawable();
        ChartGraphicsView.Drawable = _chartDrawable;

        if (_viewModel != null && _viewModel.HasData)
        {
            UpdateChart();
            Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(200), ScrollToCurrentHour);
        }
    }

    private void OnChartDataUpdated(object? sender, ChartDataUpdatedEventArgs e)
    {
        if (_isDisposed || _chartDrawable == null || e?.DataPoints == null || e.DataPoints.Count == 0)
            return;

        Dispatcher.Dispatch(() =>
        {
            CreateDayTabs();

            _chartDrawable.SetData(
                e.DataPoints,
                e.MinTemp,
                e.MaxTemp,
                e.ChartPadding,
                e.PointSpacing,
                e.CurrentHourIndex);

            ChartGraphicsView.Invalidate();

            Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(50), () =>
            {
                if (!_isDisposed && _chartDrawable != null)
                {
                    ChartGraphicsView.Invalidate();
                }
            });

            CreateDataItems();
            Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(200), ScrollToCurrentHour);
        });
    }

    private void CreateDayTabs()
    {
        if (_viewModel?.DayGroups == null || !_viewModel.DayGroups.Any() || DayTabs == null || _isDisposed)
            return;

        DayTabs.Children.Clear();

        foreach (var dayGroup in _viewModel.DayGroups)
        {
            var tabButton = new Button
            {
                Text = dayGroup.DayName,
                FontFamily = dayGroup.IsSelected ? "InterBold" : "InterMedium",
                FontSize = 14,
                BackgroundColor = Colors.Transparent,
                TextColor = dayGroup.IsSelected ? Color.FromArgb("#FFC24B") : Colors.White,
                Padding = new Thickness(12, 6),
                CornerRadius = 20,
                BorderWidth = 0,
                ClassId = dayGroup.Date.ToString(),
                Command = new Command(() =>
                {
                    _viewModel?.SelectDayCommand.Execute(dayGroup);
                })
            };

            DayTabs.Children.Add(tabButton);
        }
    }

    private void UpdateChart()
    {
        if (_viewModel == null || !_viewModel.HasData || _chartDrawable == null || _isDisposed)
            return;

        CreateDayTabs();

        _chartDrawable.SetData(
            _viewModel.CurrentDayData!,
            _viewModel.MinTemp,
            _viewModel.MaxTemp,
            0f,
            100f,
            _viewModel.CurrentHourIndex);

        ChartGraphicsView.Invalidate();
        CreateDataItems();
    }

    private void CreateDataItems()
    {
        if (_viewModel?.CurrentDayData == null || DataStack == null || _isDisposed)
            return;

        DataStack.Children.Clear();

        foreach (var hour in _viewModel.CurrentDayData)
        {
            var container = CreateHourContainer(hour);
            DataStack.Children.Add(container);
        }
    }

    private StackLayout CreateHourContainer(HourlyForecastDisplay hour)
    {
        var isCurrentHour = hour == _viewModel?.CurrentDayData?[_viewModel.CurrentHourIndex];

        var container = new StackLayout
        {
            Spacing = 4,
            HorizontalOptions = LayoutOptions.Center,
            WidthRequest = 100,
            Padding = new Thickness(0, 5)
        };

        container.Children.Add(new Label
        {
            Text = hour.TimeDisplay,
            TextColor = Colors.White,
            FontFamily = isCurrentHour ? "InterBold" : "InterMedium",
            FontSize = isCurrentHour ? 13 : 12,
            HorizontalOptions = LayoutOptions.Center
        });

        var icon = new Image
        {
            WidthRequest = 30,
            HeightRequest = 30,
            HorizontalOptions = LayoutOptions.Center
        };

        try
        {
            // Исправлено: проверка на null
            if (Application.Current?.Resources != null &&
                Application.Current.Resources.TryGetValue("WeatherIconMulti", out var converter) &&
                converter is IMultiValueConverter multiConverter)
            {
                var multiBinding = new MultiBinding
                {
                    Converter = multiConverter
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
            else
            {
                // Fallback, если конвертер не найден
                icon.Source = GetDefaultWeatherIcon(hour.IsDay);
            }
        }
        catch
        {
            icon.Source = GetDefaultWeatherIcon(hour.IsDay);
        }

        container.Children.Add(icon);

        container.Children.Add(new Label
        {
            Text = hour.TemperatureDisplay,
            TextColor = Colors.White,
            FontFamily = "InterSemiBold",
            FontSize = 16,
            HorizontalOptions = LayoutOptions.Center
        });

        container.Children.Add(new Label
        {
            Text = hour.WindSpeedDisplay,
            TextColor = Colors.White.WithAlpha(0.7f),
            FontFamily = "InterMedium",
            FontSize = 10,
            HorizontalOptions = LayoutOptions.Center
        });

        return container;
    }

    private string GetDefaultWeatherIcon(bool isDay)
    {
        return isDay ? "appic_sun.png" : "appic_moon.png";
    }

    private void ScrollToCurrentHour()
    {
        // Исправлено: проверка на null и валидность индекса
        if (_viewModel == null ||
            _viewModel.CurrentHourIndex < 0 ||
            MainScrollView == null ||
            _isDisposed)
            return;

        try
        {
            var screenWidth = Width;
            if (screenWidth <= 0) return;

            // Исправлено: проверка, что индекс не выходит за границы
            var currentDayData = _viewModel.CurrentDayData;
            if (currentDayData == null || _viewModel.CurrentHourIndex >= currentDayData.Count)
                return;

            var targetX = _viewModel.CurrentHourIndex * 100f - screenWidth / 2 + 50f;
            if (targetX < 0) targetX = 0;

            MainScrollView.ScrollToAsync(targetX, 0, true);
        }
        catch
        {
            // Ignore scroll errors
        }
    }

    public void ForceRefresh()
    {
        if (_isDisposed || _viewModel == null || !_viewModel.HasData || _chartDrawable == null)
            return;

        Dispatcher.Dispatch(() =>
        {
            if (_viewModel.CurrentDayData != null)
            {
                _chartDrawable.SetData(
                    _viewModel.CurrentDayData,
                    _viewModel.MinTemp,
                    _viewModel.MaxTemp,
                    0f,
                    100f,
                    _viewModel.CurrentHourIndex);

                ChartGraphicsView.Invalidate();
            }
        });
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        _isDisposed = true;
        Loaded -= OnLoaded;
        SizeChanged -= OnSizeChanged;

        if (_viewModel != null)
        {
            _viewModel.ChartDataUpdated -= OnChartDataUpdated;
            _viewModel.Dispose();
            _viewModel = null;
        }

        _chartDrawable = null;
        GC.SuppressFinalize(this);
    }
}