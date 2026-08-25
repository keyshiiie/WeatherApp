using WeatherApp.UI.DisplayModels;

namespace WeatherApp.UI.ViewModels;

public class ChartDrawable : IDrawable
{
    private List<HourlyForecastDisplay>? _dataPoints;
    private float _minTemp;
    private float _maxTemp;
    private float _chartPadding;
    private float _pointSpacing;
    private int _currentHourIndex = -1;

    private static readonly Color CurrentHourColor = Color.FromArgb("#FFC24B");

    public void SetData(
        List<HourlyForecastDisplay> dataPoints,
        float minTemp,
        float maxTemp,
        float chartPadding,
        float pointSpacing,
        int currentHourIndex = -1)
    {
        _dataPoints = dataPoints;
        _minTemp = minTemp;
        _maxTemp = maxTemp;
        _chartPadding = chartPadding;
        _pointSpacing = pointSpacing;
        _currentHourIndex = currentHourIndex;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (_dataPoints == null || _dataPoints.Count < 2)
            return;

        if (dirtyRect.Width <= 0 || dirtyRect.Height <= 0)
            return;

        var height = dirtyRect.Height;
        var chartHeight = height - _chartPadding * 2;
        var bottomY = _chartPadding + chartHeight;

        var points = new List<PointF>();
        for (int i = 0; i < _dataPoints.Count; i++)
        {
            var x = _chartPadding + i * _pointSpacing + _pointSpacing / 2;
            var temp = _dataPoints[i].TemperatureValue;
            var normalized = (temp - _minTemp) / (_maxTemp - _minTemp);
            var y = _chartPadding + (1 - normalized) * chartHeight;
            points.Add(new PointF(x, y));
        }

        ChartDrawable.DrawGrid(canvas, points, bottomY);
        DrawLine(canvas, points);
        DrawPoints(canvas, points);
        ChartDrawable.DrawFill(canvas, points, bottomY);
    }

    private static void DrawGrid(ICanvas canvas, List<PointF> points, float bottomY)
    {
        canvas.StrokeColor = Colors.White.WithAlpha(0.5f);
        canvas.StrokeSize = 1f;
        canvas.StrokeDashPattern = [4, 4];

        foreach (var point in points)
        {
            canvas.DrawLine(point.X, point.Y, point.X, bottomY);
        }

        canvas.StrokeDashPattern = null;
    }

    private static void DrawLine(ICanvas canvas, List<PointF> points)
    {
        canvas.StrokeColor = Colors.White;
        canvas.StrokeSize = 2.5f;

        for (int i = 0; i < points.Count - 1; i++)
        {
            canvas.DrawLine(points[i], points[i + 1]);
        }
    }

    private void DrawPoints(ICanvas canvas, List<PointF> points)
    {
        for (int i = 0; i < points.Count; i++)
        {
            var point = points[i];
            var isCurrentHour = (i == _currentHourIndex);

            if (isCurrentHour)
            {
                canvas.FillColor = CurrentHourColor.WithAlpha(0.15f);
                canvas.FillCircle(point, 14);

                canvas.StrokeColor = CurrentHourColor.WithAlpha(0.5f);
                canvas.StrokeSize = 2;
                canvas.DrawCircle(point, 10);

                canvas.FillColor = CurrentHourColor;
                canvas.FillCircle(point, 6);
            }
            else
            {
                canvas.StrokeColor = Colors.White.WithAlpha(0.3f);
                canvas.StrokeSize = 1;
                canvas.DrawCircle(point, 6);

                canvas.FillColor = Colors.White;
                canvas.FillCircle(point, 4);
            }
        }
    }

    private static void DrawFill(ICanvas canvas, List<PointF> points, float bottomY)
    {
        var path = new PathF();
        path.MoveTo(points[0].X, bottomY);

        foreach (var point in points)
        {
            path.LineTo(point);
        }

        path.LineTo(points[points.Count - 1].X, bottomY);
        path.Close();

        canvas.FillColor = Colors.White.WithAlpha(0.15f);
        canvas.FillPath(path);
    }
}