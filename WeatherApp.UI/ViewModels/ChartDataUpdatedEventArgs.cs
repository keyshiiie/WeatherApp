using System;
using System.Collections.Generic;
using System.Text;
using WeatherApp.UI.DisplayModels;

namespace WeatherApp.UI.ViewModels
{
    public class ChartDataUpdatedEventArgs : EventArgs
    {
        public List<HourlyForecastDisplay>? DataPoints { get; set; }
        public float MinTemp { get; set; }
        public float MaxTemp { get; set; }
        public int CurrentHourIndex { get; set; }
        public float ChartPadding { get; set; }
        public float PointSpacing { get; set; }
    }
}
