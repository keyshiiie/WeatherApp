using Microsoft.Maui.Graphics;
using WeatherApp.Core.Models;

namespace WeatherApp.UI.ViewModels;

public class TemperatureGraphDrawable : IDrawable
{
    public List<HourlyForecast> Data { get; set; } = new();

    private TemperatureUnit _temperatureUnit = TemperatureUnit.Celsius;
    private SpeedUnit _speedUnit = SpeedUnit.KilometersPerHour;

    public void UpdateSettings(TemperatureUnit temperatureUnit, SpeedUnit speedUnit)
    {
        _temperatureUnit = temperatureUnit;
        _speedUnit = speedUnit;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (Data == null || Data.Count < 2) return;

        // Расчет температур с учетом единиц измерения
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

            float tempValue = (float)(_temperatureUnit == TemperatureUnit.Celsius
                ? Data[i].TemperatureC
                : Data[i].TemperatureF);

            float normalized = (tempValue - (float)minTemp) / (float)range;
            float y = GraphConstants.TopPadding + (availableHeight - (availableHeight * normalized));
            points.Add(new PointF(x, y));
        }

        // --- Рисуем линию графика ---
        canvas.StrokeColor = GraphColors.LineColor;
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

        // --- Рисуем точки и карточки ---
        canvas.StrokeSize = GraphConstants.PointStrokeSize;
        for (int i = 0; i < points.Count; i++)
        {
            var p = points[i];
            canvas.StrokeColor = GraphColors.PointColor;
            canvas.DrawCircle(p.X, p.Y, 4);

            DrawCard(canvas, p.X, p.Y, Data[i]);
        }
    }

    private void DrawCard(ICanvas canvas, float x, float y, HourlyForecast data)
    {
        canvas.FontColor = GraphColors.TextColor;

        // Температура
        canvas.FontSize = GraphConstants.TemperatureFontSize;
        string tempText = _temperatureUnit == TemperatureUnit.Celsius
            ? $"{data.TemperatureC:F0}°C"
            : $"{data.TemperatureF:F0}°F";

        // Применяем отдельный цвет для температуры (основной)
        canvas.FontColor = GraphColors.TemperatureColor;
        canvas.DrawString(tempText,
            x - 20, y - GraphConstants.TempYOffset, 40, 20,
            HorizontalAlignment.Center, VerticalAlignment.Center);

        float startY = y + GraphConstants.CardStartOffset;

        // Рисуем иконку погоды (текстом/эмодзи как fallback)
        DrawWeatherFallback(canvas, x, startY, data.ConditionIcon);

        // Скорость ветра
        startY += GraphConstants.IconSpacing;
        canvas.FontSize = GraphConstants.DetailFontSize;
        // Применяем отдельный цвет для деталей (вторичный)
        canvas.FontColor = GraphColors.DetailColor;
        string windText = _speedUnit == SpeedUnit.KilometersPerHour
            ? $"{data.WindSpeedKph:F0} км/ч"
            : $"{data.WindSpeedMph:F0} миль/ч";

        canvas.DrawString(windText, x - 25, startY, 50, 15,
            HorizontalAlignment.Center, VerticalAlignment.Top);

        // Время
        startY += GraphConstants.WindSpacing;
        canvas.FontSize = GraphConstants.DetailFontSize;
        canvas.DrawString(data.TimeDisplay, x - 20, startY, 40, 15,
            HorizontalAlignment.Center, VerticalAlignment.Top);
    }

    // Метод только для отрисовки текстового смайлика (как временное решение для иконки)
    private void DrawWeatherFallback(ICanvas canvas, float x, float y, string? iconUrl)
    {
        // Устанавливаем цвет для эмодзи/иконки
        canvas.FontColor = GraphColors.TextColor;
        canvas.FontSize = GraphConstants.IconFontSize;

        // В идеале здесь надо использовать маппинг (например, Sunny -> ☀️)
        // Но если у вас нет доступа к ConditionCode, рисуем стандартный смайлик
        canvas.DrawString("☁️", x - 15, y, 30, 25,
            HorizontalAlignment.Center, VerticalAlignment.Top);
    }
}

public static class GraphConstants
{
    public const float TopPadding = 30f;
    public const float BottomLimit = 90f;

    public const float TempYOffset = 30f;
    public const float CardStartOffset = 20f;

    public const float IconSpacing = 32f;
    public const float WindSpacing = 22f;
    public const float TimeSpacing = 18f;

    public const float LineStrokeSize = 3f;
    public const float PointStrokeSize = 4f;
    public const float TemperatureFontSize = 14f;
    public const float IconFontSize = 28f;
    public const float DetailFontSize = 10f;
}

public static class GraphColors
{
    // Основные цвета
    public static Color LineColor { get; } = Color.FromArgb("#222021");
    public static Color PointColor { get; } = Color.FromArgb("#222021");
    public static Color TextColor { get; } = Color.FromArgb("#222021");
    public static Color CardBackgroundColor { get; } = Color.FromArgb("#F7F7F7"); // Фон для карточек (если будете рисовать)

    // Если хотите отдельные цвета для текста
    public static Color TemperatureColor { get; } = Color.FromArgb("#222021");
    public static Color DetailColor { get; } = Color.FromArgb("#999999"); // Цвет для ветра и времени
}