using Microsoft.Maui.Graphics;
using WeatherApp.Core.Models;

namespace WeatherApp.UI.ViewModels;

public class TemperatureGraphDrawable : IDrawable
{
    public List<HourlyForecast> Data { get; set; } = new();

    // Добавляем свойство для единиц измерения
    private TemperatureUnit _temperatureUnit = TemperatureUnit.Celsius;
    private SpeedUnit _speedUnit = SpeedUnit.KilometersPerHour;

    // Метод для обновления настроек
    public void UpdateSettings(TemperatureUnit temperatureUnit, SpeedUnit speedUnit)
    {
        _temperatureUnit = temperatureUnit;
        _speedUnit = speedUnit;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (Data == null || Data.Count < 2) return;

        // Используем текущую единицу измерения для расчета графика
        double minTemp, maxTemp;
        if (_temperatureUnit == TemperatureUnit.Celsius)
        {
            minTemp = Data.Min(h => h.TemperatureC);
            maxTemp = Data.Max(h => h.TemperatureC);
        }
        else
        {
            minTemp = Data.Min(h => h.TemperatureF);
            maxTemp = Data.Max(h => h.TemperatureF);
        }

        double range = maxTemp - minTemp;
        if (range == 0) range = 1;

        float width = dirtyRect.Width;
        float height = dirtyRect.Height;

        float availableHeight = height - GraphConstants.TopPadding - GraphConstants.BottomLimit;

        var points = new List<PointF>();
        for (int i = 0; i < Data.Count; i++)
        {
            float x = GraphConstants.TopPadding + (width - (GraphConstants.TopPadding * 2)) * (i / (float)(Data.Count - 1));

            // Используем правильную температуру для расчета Y
            float tempValue = (float)(_temperatureUnit == TemperatureUnit.Celsius
                ? Data[i].TemperatureC
                : Data[i].TemperatureF);

            float normalized = (tempValue - (float)minTemp) / (float)range;
            float y = GraphConstants.TopPadding + (availableHeight - (availableHeight * normalized));
            points.Add(new PointF(x, y));
        }

        canvas.StrokeColor = Colors.Blue;
        canvas.StrokeSize = GraphConstants.LineStrokeSize;

        if (points.Count > 1)
        {
            var path = new PathF();
            path.MoveTo(points[0]);
            for (int i = 0; i < points.Count - 1; i++)
            {
                var current = points[i];
                var next = points[i + 1];
                float midX = (current.X + next.X) / 2;
                float midY = (current.Y + next.Y) / 2;
                path.QuadTo(current.X, current.Y, midX, midY);
            }
            path.LineTo(points.Last());
            canvas.DrawPath(path);
        }

        canvas.StrokeSize = GraphConstants.PointStrokeSize;
        for (int i = 0; i < points.Count; i++)
        {
            var p = points[i];
            canvas.StrokeColor = Colors.Blue;
            canvas.DrawCircle(p.X, p.Y, 4);

            DrawCard(canvas, p.X, p.Y, Data[i]);
        }
    }

    private void DrawCard(ICanvas canvas, float x, float y, HourlyForecast data)
    {
        canvas.FontColor = Colors.White;

        // Температура с правильной единицей измерения
        canvas.FontSize = GraphConstants.TemperatureFontSize;
        string tempText = _temperatureUnit == TemperatureUnit.Celsius
            ? $"{data.TemperatureC:F0}°C"
            : $"{data.TemperatureF:F0}°F";

        canvas.DrawString(tempText,
            x - 20, y - GraphConstants.TempYOffset, 40, 20,
            HorizontalAlignment.Center, VerticalAlignment.Center);

        float startY = y + GraphConstants.CardStartOffset;
        canvas.FontSize = GraphConstants.IconFontSize;
        canvas.DrawString("☁️", x - 15, startY, 30, 25,
            HorizontalAlignment.Center, VerticalAlignment.Top);

        // Скорость ветра с правильной единицей измерения
        startY += GraphConstants.IconSpacing;
        canvas.FontSize = GraphConstants.DetailFontSize;
        string windText = _speedUnit == SpeedUnit.KilometersPerHour
            ? $"{data.WindSpeedKph:F0} км/ч"
            : $"{data.WindSpeedMph:F0} миль/ч";

        canvas.DrawString(windText, x - 25, startY, 50, 15,
            HorizontalAlignment.Center, VerticalAlignment.Top);

        startY += GraphConstants.WindSpacing;
        canvas.FontSize = GraphConstants.DetailFontSize;
        canvas.DrawString(data.TimeDisplay, x - 20, startY, 40, 15,
            HorizontalAlignment.Center, VerticalAlignment.Top);
    }
}

public static class GraphConstants
{
    // Настройки области рисования
    public const float TopPadding = 30f;
    public const float BottomLimit = 90f;

    // Настройки позиционирования текста
    public const float TempYOffset = 30f;       // Насколько поднять температуру над точкой
    public const float CardStartOffset = 20f;   // Насколько ниже точки начинать блок карточки

    // Расстояния между элементами в карточке (по вертикали)
    public const float IconSpacing = 28f;       // Отступ от температуры до иконки
    public const float WindSpacing = 28f;       // Отступ от иконки до ветра
    public const float TimeSpacing = 18f;       // Отступ от ветра до времени

    // Размеры и шрифты
    public const float LineStrokeSize = 3f;
    public const float PointStrokeSize = 4f;
    public const float TemperatureFontSize = 14f;
    public const float IconFontSize = 18f;
    public const float DetailFontSize = 10f;
}