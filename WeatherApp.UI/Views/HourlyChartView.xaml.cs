using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using WeatherApp.UI.ViewModels;
using WeatherApp.Core.Models;

namespace WeatherApp.UI.Views;

public partial class HourlyChartView : ContentView
{
    private List<HourlyForecastDisplay>? _dataPoints;
    private float _minTemp;
    private float _maxTemp;
    private ChartDrawable? _chartDrawable;
    private const float ChartPadding = 20f; // Константа для отступа
    private const float PointSpacing = 100f; // Расстояние между точками

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

        // Вычисляем общую ширину
        var totalWidth = ChartPadding * 2 + (_dataPoints.Count - 1) * PointSpacing + 20;

        // Обновляем ширину контейнеров
        if (ChartGraphicsView != null)
        {
            ChartGraphicsView.WidthRequest = totalWidth;
        }

        if (DataStack != null)
        {
            DataStack.WidthRequest = totalWidth - ChartPadding * 2;
        }

        // Обновляем график
        if (_chartDrawable == null)
            _chartDrawable = new ChartDrawable();

        _chartDrawable.SetData(_dataPoints, _minTemp, _maxTemp);

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

        // Устанавливаем отступы для каждого элемента, чтобы он был точно под точкой
        for (int i = 0; i < _dataPoints.Count; i++)
        {
            var hour = _dataPoints[i];
            
            // Создаем контейнер для каждого часа
            var container = new StackLayout
            {
                Spacing = 4,
                HorizontalOptions = LayoutOptions.Center,
                WidthRequest = 85,
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

            // Используем ваш конвертер для иконок
            try
            {
                var multiBinding = new MultiBinding
                {
                    Converter = Application.Current.Resources["WeatherIconMulti"] as IMultiValueConverter
                };

                var conditionBinding = new Binding
                {
                    Source = hour,
                    Path = "ConditionCode"
                };

                var isDayBinding = new Binding
                {
                    Source = hour,
                    Path = "IsDay"
                };

                multiBinding.Bindings.Add(conditionBinding);
                multiBinding.Bindings.Add(isDayBinding);

                icon.SetBinding(Image.SourceProperty, multiBinding);
            }
            catch
            {
                // Если конвертер не работает, используем заглушку
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

            // Добавляем элемент с отступом, чтобы центрировать под точкой
            // Ширина элемента 85px, расстояние между точками 100px
            // Центрируем: (100 - 85) / 2 = 7.5px
            var wrapper = new Grid
            {
                WidthRequest = PointSpacing,
                HorizontalOptions = LayoutOptions.Center
            };
            
            wrapper.Children.Add(container);
            
            // Центрируем контейнер внутри Grid
            container.HorizontalOptions = LayoutOptions.Center;
            
            DataStack.Children.Add(wrapper);
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