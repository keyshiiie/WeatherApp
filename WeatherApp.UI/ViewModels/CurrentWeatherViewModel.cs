using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeatherApp.Core.Models;
using WeatherApp.Core.Services;

namespace WeatherApp.UI.ViewModels;

public partial class CurrentWeatherViewModel : BaseViewModel
{
    private readonly IWeatherService _weatherService;
    private readonly ICityService _cityService;
    private readonly ISettingsService _settingsService;

    [ObservableProperty]
    private WeatherData? _currentWeather;

    [ObservableProperty]
    private List<ForecastDay>? _forecastDays;

    [ObservableProperty]
    private City? _selectedCity;

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private bool _isCurrentCityFavorite;

    [ObservableProperty]
    private UserSettings _settings = new();

    [ObservableProperty]
    private TemperatureGraphDrawable _temperatureGraphDrawable = new();

    [ObservableProperty]
    private List<HourlyForecast> _hourlyForecast = new();

    public CurrentWeatherViewModel(
        IWeatherService weatherService,
        ICityService cityService,
        ISettingsService settingsService)
    {
        _weatherService = weatherService ?? throw new ArgumentNullException(nameof(weatherService));
        _cityService = cityService ?? throw new ArgumentNullException(nameof(cityService));
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));

        Title = "Погода";

        _settingsService.SettingsChanged += OnSettingsChanged;
        Settings = _settingsService.GetSettings();
    }

    private void OnSettingsChanged(object? sender, UserSettings settings)
    {
        Settings = settings;

        TemperatureGraphDrawable.UpdateSettings(Settings.TemperatureUnit, Settings.SpeedUnit);
        OnPropertyChanged(nameof(TemperatureGraphDrawable));

        OnPropertyChanged(nameof(TemperatureDisplay));
        OnPropertyChanged(nameof(FeelsLikeDisplay));
        OnPropertyChanged(nameof(PressureDisplay));
        OnPropertyChanged(nameof(WindSpeedDisplay));
        OnPropertyChanged(nameof(MinTempDisplay));
        OnPropertyChanged(nameof(MaxTempDisplay));
    }

    #region Display Properties

    public string TemperatureDisplay
    {
        get
        {
            if (CurrentWeather == null) return "--";
            return Settings.TemperatureUnit == TemperatureUnit.Celsius
                ? $"{CurrentWeather.TemperatureC:F0}°C"
                : $"{CurrentWeather.TemperatureF:F0}°F";
        }
    }

    public string FeelsLikeDisplay
    {
        get
        {
            if (CurrentWeather == null) return "--";
            return Settings.TemperatureUnit == TemperatureUnit.Celsius
                ? $"{CurrentWeather.FeelsLikeC:F0}°C"
                : $"{CurrentWeather.FeelsLikeF:F0}°F";
        }
    }

    public string PressureDisplay
    {
        get
        {
            if (CurrentWeather == null) return "--";
            return Settings.PressureUnit == PressureUnit.Millibars
                ? $"{CurrentWeather.PressureMb:F0} мбар"
                : $"{CurrentWeather.PressureIn:F2} inHg";
        }
    }

    public string WindSpeedDisplay
    {
        get
        {
            if (CurrentWeather == null) return "--";
            return Settings.SpeedUnit == SpeedUnit.KilometersPerHour
                ? $"{CurrentWeather.WindSpeedKph:F0} км/ч"
                : $"{CurrentWeather.WindSpeedMph:F0} миль/ч";
        }
    }

    public string MinTempDisplay
    {
        get
        {
            if (ForecastDays == null || !ForecastDays.Any()) return "--";
            var today = ForecastDays.FirstOrDefault();
            if (today == null) return "--";
            return Settings.TemperatureUnit == TemperatureUnit.Celsius
                ? $"{today.MinTempC:F0}°C"
                : $"{today.MinTempF:F0}°F";
        }
    }

    public string MaxTempDisplay
    {
        get
        {
            if (ForecastDays == null || !ForecastDays.Any()) return "--";
            var today = ForecastDays.FirstOrDefault();
            if (today == null) return "--";
            return Settings.TemperatureUnit == TemperatureUnit.Celsius
                ? $"{today.MaxTempC:F0}°C"
                : $"{today.MaxTempF:F0}°F";
        }
    }

    #endregion

    #region Commands (генерируются автоматически)

    [RelayCommand]
    private async Task RefreshWeatherAsync()
    {
        if (IsRefreshing || SelectedCity == null) return;

        try
        {
            IsRefreshing = true;
            await LoadWeatherForCityAsync(SelectedCity);
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task AddToFavoritesAsync()
    {
        if (CurrentWeather == null || string.IsNullOrEmpty(CurrentWeather.CityName)) return;

        await ExecuteAsync(async () =>
        {
            var city = new City
            {
                Name = CurrentWeather.CityName,
                Country = CurrentWeather.Country,
                Region = CurrentWeather.Region,
                Latitude = CurrentWeather.Latitude,
                Longitude = CurrentWeather.Longitude,
                AddedAt = DateTime.UtcNow,
                IsLastSelected = false,
                IsFavorite = true,
                IsRecent = false
            };

            await _cityService.AddFavoriteAsync(city);
            await CheckIsFavoriteAsync();
            UpdateFavoriteToolbarItem();
        }, "Не удалось добавить в избранное");
    }

    [RelayCommand]
    private async Task RemoveFromFavoritesAsync()
    {
        if (CurrentWeather == null || string.IsNullOrEmpty(CurrentWeather.CityName)) return;

        await ExecuteAsync(async () =>
        {
            await _cityService.RemoveFavoriteByNameAsync(CurrentWeather.CityName);
            await CheckIsFavoriteAsync();
            UpdateFavoriteToolbarItem();
        }, "Не удалось удалить из избранного");
    }

    [RelayCommand]
    private async Task ToggleFavoriteAsync()
    {
        if (IsCurrentCityFavorite)
            await RemoveFromFavoritesAsync();
        else
            await AddToFavoritesAsync();
    }

    #endregion

    #region Public Methods

    public override async Task OnAppearingAsync()
    {
        Settings = _settingsService.GetSettings();
        if (SelectedCity != null)
        {
            await LoadWeatherForCityAsync(SelectedCity);
        }
        else
        {
            await LoadBestCityAsync();
        }
    }

    public async Task LoadWeatherForCityAsync(City city)
    {
        if (city == null) return;

        IsCurrentCityFavorite = false;
        SelectedCity = city;

        await ExecuteAsync(async () =>
        {
            var (current, forecast) = await _weatherService.GetCurrentAndForecastAsync(
                city.Latitude,
                city.Longitude,
                5);

            if (current != null)
            {
                current.CityName = city.Name;
                current.Country = city.Country;
                current.Region = city.Region;

                Title = $"Погода в {city.DisplayName}";

                CurrentWeather = current;
                ForecastDays = forecast;

                if (forecast != null)
                {
                    HourlyForecast = forecast.SelectMany(d => d.Hours).OrderBy(h => h.Time).ToList();

                    TemperatureGraphDrawable.UpdateSettings(Settings.TemperatureUnit, Settings.SpeedUnit);
                    TemperatureGraphDrawable.Data = HourlyForecast;
                    OnPropertyChanged(nameof(TemperatureGraphDrawable));
                }
                else
                {
                    HourlyForecast = new List<HourlyForecast>();
                }

                OnPropertyChanged(nameof(TemperatureDisplay));
                OnPropertyChanged(nameof(FeelsLikeDisplay));
                OnPropertyChanged(nameof(PressureDisplay));
                OnPropertyChanged(nameof(WindSpeedDisplay));
                OnPropertyChanged(nameof(MinTempDisplay));
                OnPropertyChanged(nameof(MaxTempDisplay));

                await CheckIsFavoriteAsync();
                UpdateFavoriteToolbarItem();
            }
            else
            {
                SetError($"Не удалось загрузить погоду для {city.Name}");
            }
        }, $"Не удалось загрузить погоду для {city.Name}");
    }

    public async Task CheckIsFavoriteAsync()
    {
        if (CurrentWeather != null && !string.IsNullOrEmpty(CurrentWeather.CityName))
        {
            IsCurrentCityFavorite = await _cityService.IsFavoriteAsync(CurrentWeather.CityName);
        }
        else
        {
            IsCurrentCityFavorite = false;
        }
    }

    private void UpdateFavoriteToolbarItem()
    {
        var toolbarItem = Shell.Current?.CurrentPage?.FindByName<ToolbarItem>("FavoriteToolbarItem");

        if (toolbarItem != null)
        {
            if (IsCurrentCityFavorite)
            {
                toolbarItem.Text = "Удалить";
                toolbarItem.Command = RemoveFromFavoritesCommand;
            }
            else
            {
                toolbarItem.Text = "В избранное";
                toolbarItem.Command = AddToFavoritesCommand;
            }
        }
    }

    #endregion

    #region Private Methods

    private async Task LoadBestCityAsync()
    {
        try
        {
            var bestCity = await _cityService.GetBestCityAsync();

            if (bestCity != null)
            {
                await LoadWeatherForCityAsync(bestCity);
            }
            else
            {
                var defaultCity = new City
                {
                    Name = "Москва",
                    Country = "Россия",
                    Region = "Московская область",
                    Latitude = 55.7558,
                    Longitude = 37.6176,
                    AddedAt = DateTime.UtcNow,
                    IsLastSelected = false,
                    IsFavorite = false,
                    IsRecent = false
                };

                await LoadWeatherForCityAsync(defaultCity);
            }
        }
        catch (Exception ex)
        {
            SetError($"Не удалось загрузить начальный город: {ex.Message}");
        }
    }

    #endregion
}