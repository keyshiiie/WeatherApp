using CommunityToolkit.Mvvm.ComponentModel;
using WeatherApp.Core.Models;

namespace WeatherApp.UI.DisplayModels;

/// <summary>
/// Модель для отображения избранного города с погодой
/// </summary>
public partial class FavoriteCityDisplay : WeatherDisplay
{
    private readonly City _city;
    private WeatherData? _weather;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasError;

    public FavoriteCityDisplay(City city, UserSettings settings)
        : base(settings)
    {
        _city = city ?? throw new ArgumentNullException(nameof(city));
    }

    public City City => _city;
    public string DisplayName => _city.DisplayName;

    public WeatherData? Weather
    {
        get => _weather;
        set
        {
            if (_weather != value)
            {
                _weather = value;
                OnPropertyChanged(nameof(Weather));
                OnPropertyChanged(nameof(TemperatureDisplay));
                OnPropertyChanged(nameof(ConditionText));
                OnPropertyChanged(nameof(ConditionCode));
                OnPropertyChanged(nameof(IsDay));
                OnPropertyChanged(nameof(HasWeather));
            }
        }
    }

    public bool HasWeather => _weather != null;

    public string TemperatureDisplay => _weather != null
        ? FormatTemperature(_weather.TemperatureC, _weather.TemperatureF)
        : "--°";

    public string ConditionText => _weather?.ConditionText ?? "Нет данных";

    // Свойства для WeatherIconMultiConverter
    public int ConditionCode => _weather?.ConditionCode ?? 1000;
    public bool IsDay => _weather?.IsDay ?? true;
}