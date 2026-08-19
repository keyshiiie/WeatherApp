using WeatherApp.UI.ViewModels;

namespace WeatherApp.UI.Views
{
    public class ChartDrawable : IDrawable
    {
        private List<HourlyForecastDisplay>? _dataPoints;
        private float _minTemp;
        private float _maxTemp;
        private float _chartPadding;
        private float _pointSpacing;

        public void SetData(List<HourlyForecastDisplay> dataPoints, float minTemp, float maxTemp,
                            float chartPadding, float pointSpacing)
        {
            _dataPoints = dataPoints;
            _minTemp = minTemp;
            _maxTemp = maxTemp;
            _chartPadding = chartPadding;
            _pointSpacing = pointSpacing;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (_dataPoints == null || _dataPoints.Count < 2)
                return;

            var height = dirtyRect.Height;
            var chartHeight = height - _chartPadding * 2;
            var bottomY = _chartPadding + chartHeight; // Нижняя граница графика

            // Вычисляем точки графика
            var points = new List<PointF>();

            for (int i = 0; i < _dataPoints.Count; i++)
            {
                // Центр блока данных
                var x = _chartPadding + i * _pointSpacing + _pointSpacing / 2;

                var temp = _dataPoints[i].TemperatureValue;
                var normalized = (temp - _minTemp) / (_maxTemp - _minTemp);
                var y = _chartPadding + (1 - normalized) * chartHeight;

                points.Add(new PointF(x, y));
            }

            // вертикальная пунктирная линия ===
            canvas.StrokeColor = Colors.White.WithAlpha(0.5f); // Полупрозрачные
            canvas.StrokeSize = 1f;
            canvas.StrokeDashPattern = new float[] { 4, 4 }; // Пунктир: 4px линия, 4px промежуток

            foreach (var point in points)
            {
                canvas.DrawLine(point.X, point.Y, point.X, bottomY);
            }

            // Сбрасываем пунктир для остальных элементов
            canvas.StrokeDashPattern = null;

            // линия графика ===
            canvas.StrokeColor = Colors.White;
            canvas.StrokeSize = 2.5f;

            for (int i = 0; i < points.Count - 1; i++)
            {
                canvas.DrawLine(points[i], points[i + 1]);
            }

            // точки на графике
            foreach (var point in points)
            {
                canvas.FillColor = Colors.White;
                canvas.FillCircle(point, 4);

                canvas.StrokeColor = Colors.White.WithAlpha(0.3f);
                canvas.StrokeSize = 1;
                canvas.DrawCircle(point, 6);
            }

            // заливка под графиком
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
}