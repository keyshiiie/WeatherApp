using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WeatherApp.Core.Models;
using WeatherApp.Core.Services;
using WeatherApp.UI.DisplayModels;

namespace WeatherApp.UI.ViewModels;

public partial class CurrentWeatherViewModel : BaseViewModel
{
    public event EventHandler<List<HourlyForecastDisplay>>? HourlyDataUpdated;
    private readonly IWeatherService _weatherService;
    private readonly ICityService _cityService;
    private readonly ISettingsService _settingsService;

    [ObservableProperty]
    private WeatherData? _currentWeather;

    [ObservableProperty]
    private CurrentWeatherDisplay? _currentWeatherDisplay;

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
    private List<HourlyForecast> _hourlyForecast = new();

    [ObservableProperty]
    private List<ForecastDayDisplay>? _forecastDaysDisplay;

    [ObservableProperty]
    private List<HourlyForecastDisplay>? _hourlyForecastDisplay;

    public CurrentWeatherViewModel(
        IWeatherService weatherService,
        ICityService cityService,
        ISettingsService settingsService,
        ILogger<CurrentWeatherViewModel> logger)
        : base(logger)
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
        UpdateDisplayModels();
    }

    #region Commands

    [RelayCommand]
    private async Task RefreshWeatherAsync()
    {
        Logger.LogInformation("Refreshing weather");

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
        Logger.LogInformation("Going back");
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task AddToFavoritesAsync()
    {
        if (CurrentWeather == null || string.IsNullOrEmpty(CurrentWeather.CityName))
        {
            Logger.LogWarning("Cannot add to favorites: current weather is null or city name is empty");
            return;
        }

        Logger.LogInformation($"Adding {CurrentWeather.CityName} to favorites");

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

            Logger.LogInformation($"Added {CurrentWeather.CityName} to favorites");
        }, "Не удалось добавить в избранное");
    }

    [RelayCommand]
    private async Task RemoveFromFavoritesAsync()
    {
        if (CurrentWeather == null || string.IsNullOrEmpty(CurrentWeather.CityName))
        {
            Logger.LogWarning("Cannot remove from favorites: current weather is null or city name is empty");
            return;
        }

        Logger.LogInformation($"Removing {CurrentWeather.CityName} from favorites");

        await ExecuteAsync(async () =>
        {
            await _cityService.RemoveFavoriteByNameAsync(CurrentWeather.CityName);
            await CheckIsFavoriteAsync();
            UpdateFavoriteToolbarItem();

            Logger.LogInformation($"Removed {CurrentWeather.CityName} from favorites");
        }, "Не удалось удалить из избранного");
    }

    [RelayCommand]
    private async Task ToggleFavoriteAsync()
    {
        Logger.LogInformation($"Toggling favorite for {CurrentWeather?.CityName}");

        if (IsCurrentCityFavorite)
            await RemoveFromFavoritesAsync();
        else
            await AddToFavoritesAsync();
    }

    #endregion

    #region Public Methods

    public override async Task OnAppearingAsync()
    {
        Logger.LogInformation("CurrentWeather page appearing");

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
        if (city == null)
        {
            Logger.LogWarning("LoadWeatherForCityAsync called with null city");
            return;
        }

        Logger.LogInformation($"Loading weather for city: {city.Name} ({city.Latitude}, {city.Longitude})");

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

                Title = city.DisplayName;

                // Просто создаем Display модели
                CurrentWeather = current;
                CurrentWeatherDisplay = new CurrentWeatherDisplay(current, Settings);
                ForecastDays = forecast;

                if (forecast != null && forecast.Any())
                {
                    HourlyForecast = forecast.SelectMany(d => d.Hours).OrderBy(h => h.Time).ToList();

                    // Создаем списки Display моделей
                    ForecastDaysDisplay = forecast
                        .Select(day => new ForecastDayDisplay(day, Settings))
                        .ToList();

                    HourlyForecastDisplay = HourlyForecast
                        .Select(hour => new HourlyForecastDisplay(hour, Settings))
                        .ToList();

                    // Вызываем событие для графика
                    HourlyDataUpdated?.Invoke(this, HourlyForecastDisplay);

                    Logger.LogInformation($"Loaded weather for {city.Name}: {current.TemperatureC}°C, {forecast.Count} forecast days");
                }
                else
                {
                    HourlyForecast = new List<HourlyForecast>();
                    ForecastDaysDisplay = new List<ForecastDayDisplay>();
                    HourlyForecastDisplay = new List<HourlyForecastDisplay>();
                    Logger.LogWarning($"No forecast data for {city.Name}");
                }

                // Уведомляем об обновлении
                OnPropertyChanged(nameof(CurrentWeather));
                OnPropertyChanged(nameof(CurrentWeatherDisplay));
                OnPropertyChanged(nameof(ForecastDays));
                OnPropertyChanged(nameof(ForecastDaysDisplay));
                OnPropertyChanged(nameof(HourlyForecast));
                OnPropertyChanged(nameof(HourlyForecastDisplay));

                await CheckIsFavoriteAsync();
                UpdateFavoriteToolbarItem();
            }
            else
            {
                Logger.LogError($"Failed to load weather for {city.Name}");
                SetError($"Не удалось загрузить погоду для {city.Name}");
            }
        }, $"Не удалось загрузить погоду для {city.Name}");
    }

    public async Task CheckIsFavoriteAsync()
    {
        if (CurrentWeather != null && !string.IsNullOrEmpty(CurrentWeather.CityName))
        {
            IsCurrentCityFavorite = await _cityService.IsFavoriteAsync(CurrentWeather.CityName);
            Logger.LogDebug($"Is {CurrentWeather.CityName} favorite: {IsCurrentCityFavorite}");
        }
        else
        {
            IsCurrentCityFavorite = false;
        }
    }

    #endregion

    #region Private Methods

    private void UpdateDisplayModels()
    {
        // Обновляем все Display модели при изменении настроек
        if (CurrentWeather != null)
        {
            CurrentWeatherDisplay = new CurrentWeatherDisplay(CurrentWeather, Settings);
        }

        if (ForecastDays != null && ForecastDays.Any())
        {
            ForecastDaysDisplay = ForecastDays
                .Select(day => new ForecastDayDisplay(day, Settings))
                .ToList();
        }
        else
        {
            ForecastDaysDisplay = new List<ForecastDayDisplay>();
        }

        if (HourlyForecast != null && HourlyForecast.Any())
        {
            HourlyForecastDisplay = HourlyForecast
                .Select(hour => new HourlyForecastDisplay(hour, Settings))
                .ToList();

            HourlyDataUpdated?.Invoke(this, HourlyForecastDisplay);
        }
        else
        {
            HourlyForecastDisplay = new List<HourlyForecastDisplay>();
        }

        OnPropertyChanged(nameof(CurrentWeatherDisplay));
        OnPropertyChanged(nameof(ForecastDaysDisplay));
        OnPropertyChanged(nameof(HourlyForecastDisplay));
    }

    private void UpdateFavoriteToolbarItem()
    {
        try
        {
            var toolbarItem = Shell.Current?.CurrentPage?.FindByName<ToolbarItem>("FavoriteToolbarItem");

            if (toolbarItem != null)
            {
                var isDark = Application.Current?.RequestedTheme == AppTheme.Dark;

                string iconName;
                if (IsCurrentCityFavorite)
                {
                    iconName = isDark ? "appic_heart_filled_light.png" : "appic_heart_filled_dark.png";
                }
                else
                {
                    iconName = isDark ? "appic_heart_outline_light.png" : "appic_heart_outline_dark.png";
                }

                toolbarItem.IconImageSource = iconName;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating favorite toolbar item");
        }
    }

    private async Task LoadBestCityAsync()
    {
        Logger.LogInformation("Loading best city");

        try
        {
            var bestCity = await _cityService.GetBestCityAsync();

            if (bestCity != null)
            {
                Logger.LogInformation($"Best city found: {bestCity.Name}");
                await LoadWeatherForCityAsync(bestCity);
            }
            else
            {
                Logger.LogWarning("No best city found, using default (Moscow)");

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
            Logger.LogError(ex, "Error loading best city");
            SetError($"Не удалось загрузить начальный город: {ex.Message}");
        }
    }

    #endregion
}