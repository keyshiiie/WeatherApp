using System;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;
using WeatherApp.UI.ViewModels;

namespace WeatherApp.UI.Views
{
    public class ChartDrawable : IDrawable
    {
        private List<HourlyForecastDisplay>? _dataPoints;
        private float _minTemp;
        private float _maxTemp;

        public void SetData(List<HourlyForecastDisplay> dataPoints, float minTemp, float maxTemp)
        {
            _dataPoints = dataPoints;
            _minTemp = minTemp;
            _maxTemp = maxTemp;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (_dataPoints == null || _dataPoints.Count < 2)
                return;

            var width = dirtyRect.Width;
            var height = dirtyRect.Height;
            var padding = 20f;
            var chartWidth = width - padding * 2;
            var chartHeight = height - padding * 2;

            var pointSpacing = 100f; // Расстояние между точками
            var totalWidth = (_dataPoints.Count - 1) * pointSpacing;

            var startX = padding;

            var points = new List<PointF>();

            for (int i = 0; i < _dataPoints.Count; i++)
            {
                var x = startX + i * pointSpacing;
                var temp = _dataPoints[i].TemperatureValue;

                // Нормализуем температуру в диапазон [0, 1]
                var normalized = (temp - _minTemp) / (_maxTemp - _minTemp);
                // Инвертируем Y (0 = верх, height = низ)
                var y = padding + (1 - normalized) * chartHeight;

                points.Add(new PointF(x, y));
            }

            //// Рисуем сетку
            //DrawGrid(canvas, dirtyRect, padding);

            // Рисуем линию
            canvas.StrokeColor = Colors.White;
            canvas.StrokeSize = 2.5f;

            for (int i = 0; i < points.Count - 1; i++)
            {
                canvas.DrawLine(points[i], points[i + 1]);
            }

            // Рисуем точки
            foreach (var point in points)
            {
                canvas.FillColor = Colors.White;
                canvas.FillCircle(point, 4);

                // Добавляем ободок для лучшей видимости
                canvas.StrokeColor = Colors.White.WithAlpha(0.3f);
                canvas.StrokeSize = 1;
                canvas.DrawCircle(point, 6);
            }

            // Заливка под графиком
            var path = new PathF();
            path.MoveTo(points[0].X, padding + chartHeight);

            foreach (var point in points)
            {
                path.LineTo(point);
            }

            path.LineTo(points[points.Count - 1].X, padding + chartHeight);
            path.Close();

            // Используем полупрозрачную заливку
            canvas.FillColor = Colors.White.WithAlpha(0.15f);
            canvas.FillPath(path);
        }

        private void DrawGrid(ICanvas canvas, RectF dirtyRect, float padding)
        {
            var height = dirtyRect.Height;
            var chartHeight = height - padding * 2;
            var width = dirtyRect.Width;

            // Горизонтальные линии (каждые 10% от высоты)
            canvas.StrokeColor = Colors.White.WithAlpha(0.15f);
            canvas.StrokeSize = 1;
            canvas.StrokeDashPattern = new float[] { 5, 5 };

            for (int i = 0; i <= 10; i++)
            {
                var y = padding + (1 - i / 10f) * chartHeight;
                canvas.DrawLine(padding, y, width - padding, y);
            }

            // Добавляем вертикальные линии для каждого дня
            if (_dataPoints != null)
            {
                canvas.StrokeColor = Colors.White.WithAlpha(0.08f);
                canvas.StrokeDashPattern = new float[] { 2, 4 };

                for (int i = 0; i < _dataPoints.Count; i += 24) // Каждые 24 часа
                {
                    if (i < _dataPoints.Count)
                    {
                        var x = padding + i * 100f;
                        if (x < width - padding)
                        {
                            canvas.DrawLine(x, padding, x, padding + chartHeight);
                        }
                    }
                }
            }
        }
    }
}