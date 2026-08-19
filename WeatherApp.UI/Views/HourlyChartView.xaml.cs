using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using WeatherApp.Core.Models;
using WeatherApp.UI.DisplayModels;

namespace WeatherApp.UI.Views;

public partial class HourlyChartView : ContentView
{
    private List<HourlyForecastDisplay>? _dataPoints;
    private float _minTemp;
    private float _maxTemp;
    private ChartDrawable? _chartDrawable;

    // Настройки
    private const float ChartPadding = 0f;
    private const float PointSpacing = 100f;
    private const float DataBlockWidth = PointSpacing;
    private const float RightPadding = PointSpacing / 2;

    public HourlyChartView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        if (_chartDrawable == null)
        {
            _chartDrawable = new ChartDrawable();
            if (ChartGraphicsView != null)
            {
                ChartGraphicsView.Drawable = _chartDrawable;
            }
        }
    }

    public void SetData(List<HourlyForecastDisplay> dataPoints)
    {
        _dataPoints = dataPoints;

        if (!IsLoaded)
        {
            Loaded += (s, e) => UpdateChart();
            return;
        }

        UpdateChart();
    }

    private void UpdateChart()
    {
        if (_dataPoints == null || !_dataPoints.Any())
            return;

        // Находим min/max температуры
        var temps = _dataPoints.Select(d => d.TemperatureValue).ToList();
        _minTemp = temps.Min();
        _maxTemp = temps.Max();

        var padding = (_maxTemp - _minTemp) * 0.1f;
        if (padding < 1) padding = 1;
        _minTemp -= padding;
        _maxTemp += padding;

        // Вычисляем общую ширину с учетом правого отступа
        // Для последней точки нужен отступ, чтобы она не обрезалась
        var totalWidth = ChartPadding + (_dataPoints.Count - 1) * PointSpacing + PointSpacing; // + PointSpacing для последней точки

        // Обновляем ширину графика
        if (ChartGraphicsView != null)
        {
            ChartGraphicsView.WidthRequest = totalWidth;
        }

        // Обновляем график
        if (_chartDrawable == null)
            _chartDrawable = new ChartDrawable();

        _chartDrawable.SetData(_dataPoints, _minTemp, _maxTemp, ChartPadding, PointSpacing);

        if (ChartGraphicsView != null)
        {
            ChartGraphicsView.Drawable = _chartDrawable;
            ChartGraphicsView.Invalidate();
        }

        // Обновляем список данных
        UpdateDataList();
    }

    private void UpdateDataList()
    {
        if (DataStack == null || _dataPoints == null)
            return;

        DataStack.Children.Clear();

        // Добавляем отступ слева
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

        // Создаем блоки данных
        for (int i = 0; i < _dataPoints.Count; i++)
        {
            var hour = _dataPoints[i];

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
                FontFamily = "InterMedium",
                FontSize = 12,
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

            // Температура
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

        // Добавляем отступ справа (такой же как слева)
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

    public void RefreshData()
    {
        if (_dataPoints != null)
        {
            UpdateChart();
        }
    }
}