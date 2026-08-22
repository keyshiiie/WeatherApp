using CommunityToolkit.Mvvm.ComponentModel;
using WeatherApp.UI.DisplayModels;

namespace WeatherApp.UI.ViewModels
{
    public class DayGroup : ObservableObject
    {
        private bool _isSelected;
        private int _index;

        public DateTime Date { get; set; }
        public string? DayName { get; set; }
        public List<HourlyForecastDisplay>? HourlyData { get; set; }

        public int Index
        {
            get => _index;
            set => SetProperty(ref _index, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}
