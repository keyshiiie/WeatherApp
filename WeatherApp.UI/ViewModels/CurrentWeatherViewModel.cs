using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WeatherApp.Core.Models;
using WeatherApp.Core.Results;
using WeatherApp.Core.Services;
using WeatherApp.UI.DisplayModels;
using WeatherApp.UI.Services;

namespace WeatherApp.UI.ViewModels;

public partial class CurrentWeatherViewModel : BaseViewModel, IDisposable
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
        INavigationService navigationService,
        ILogger<CurrentWeatherViewModel> logger)
        : base(logger, navigationService)
    {
        _weatherService = weatherService ?? throw new ArgumentNullException(nameof(weatherService));
        _cityService = cityService ?? throw new ArgumentNullException(nameof(cityService));
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));

        Title = "Погода";

        _settingsService.SettingsChanged += OnSettingsChanged;
        var settingsResult = _settingsService.GetSettings();
        if (settingsResult.IsSuccess)
        {
            Settings = settingsResult.Value!;
        }
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
        await NavigationService.GoBackAsync();
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

        var result = await ExecuteWithResultAsync(
            async () =>
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

                var addResult = await _cityService.AddFavoriteAsync(city);
                if (addResult.IsFailure)
                    return Result.Failure(addResult.Error!);

                if (SelectedCity != null && addResult.Value != null)
                {
                    SelectedCity.Id = addResult.Value.Id;
                }

                await CheckIsFavoriteAsync();
                UpdateFavoriteToolbarItem();

                Logger.LogInformation($"Added {CurrentWeather.CityName} to favorites");
                return Result.Success();
            },
            successMessage: $"{CurrentWeather.CityName} добавлен в избранное",
            errorMessage: "Не удалось добавить в избранное"
        );
    }

    [RelayCommand]
    private async Task RemoveFromFavoritesAsync()
    {
        if (SelectedCity == null)
        {
            Logger.LogWarning("Cannot remove from favorites: SelectedCity is null");
            return;
        }

        if (SelectedCity.Id == 0)
        {
            Logger.LogWarning($"Cannot remove from favorites: City '{SelectedCity.Name}' has no ID");
            await ShowAlertAsync("Ошибка", "Город не сохранен в базе данных. Попробуйте обновить страницу.");
            return;
        }

        Logger.LogInformation($"Removing {SelectedCity.Name} from favorites (ID: {SelectedCity.Id})");

        var result = await ExecuteWithResultAsync(
            async () =>
            {
                var removeResult = await _cityService.RemoveFavoriteAsync(SelectedCity.Id);
                if (removeResult.IsFailure)
                    return Result.Failure(removeResult.Error!);

                await CheckIsFavoriteAsync();
                UpdateFavoriteToolbarItem();

                Logger.LogInformation($"Removed {SelectedCity.Name} from favorites");
                return Result.Success();
            },
            successMessage: $"{SelectedCity.Name} удален из избранного",
            errorMessage: "Не удалось удалить из избранного"
        );
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

        var settingsResult = _settingsService.GetSettings();
        if (settingsResult.IsSuccess)
        {
            Settings = settingsResult.Value!;
        }

        if (SelectedCity != null)
        {
            await LoadWeatherForCityAsync(SelectedCity);
        }
    }

    public async Task LoadWeatherForCityAsync(City city)
    {
        if (city == null)
        {
            Logger.LogWarning("LoadWeatherForCityAsync called with null city");
            return;
        }

        if (string.IsNullOrEmpty(city.Name))
        {
            Logger.LogWarning("LoadWeatherForCityAsync called with city without name");
            await ShowAlertAsync("Ошибка", "Название города не указано");
            return;
        }

        Logger.LogInformation($"Loading weather for city: {city.Name} ({city.Latitude}, {city.Longitude})");

        IsCurrentCityFavorite = false;
        SelectedCity = city;

        var result = await ExecuteWithResultAsync<WeatherData>(
            async () =>
            {
                var weatherResult = await _weatherService.GetCurrentAndForecastAsync(
                    city.Latitude,
                    city.Longitude,
                    5);

                if (weatherResult.IsFailure)
                    return Result.Failure<WeatherData>(weatherResult.Error!);

                var (current, forecast) = weatherResult.Value;
                if (current == null)
                {
                    return Result.Failure<WeatherData>(new NotFoundError("Weather", city.Name));
                }

                current.CityName = city.Name;
                current.Country = city.Country;
                current.Region = city.Region;

                Title = city.DisplayName;

                CurrentWeather = current;
                CurrentWeatherDisplay = new CurrentWeatherDisplay(current, Settings);
                ForecastDays = forecast;

                if (forecast != null && forecast.Any())
                {
                    HourlyForecast = forecast.SelectMany(d => d.Hours).OrderBy(h => h.Time).ToList();

                    ForecastDaysDisplay = forecast
                        .Select(day => new ForecastDayDisplay(day, Settings))
                        .ToList();

                    HourlyForecastDisplay = HourlyForecast
                        .Select(hour => new HourlyForecastDisplay(hour, Settings))
                        .ToList();

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

                OnPropertyChanged(nameof(CurrentWeather));
                OnPropertyChanged(nameof(CurrentWeatherDisplay));
                OnPropertyChanged(nameof(ForecastDays));
                OnPropertyChanged(nameof(ForecastDaysDisplay));
                OnPropertyChanged(nameof(HourlyForecast));
                OnPropertyChanged(nameof(HourlyForecastDisplay));

                await CheckIsFavoriteAsync();
                UpdateFavoriteToolbarItem();

                return Result.Success(current);
            },
            errorMessage: $"Не удалось загрузить погоду для {city.Name}"
        );

        if (result.IsFailure)
        {
            SetError(result.Error!);
        }
    }

    public async Task CheckIsFavoriteAsync()
    {
        if (CurrentWeather != null && !string.IsNullOrEmpty(CurrentWeather.CityName))
        {
            var result = await _cityService.IsFavoriteAsync(CurrentWeather.CityName);
            if (result.IsSuccess)
            {
                IsCurrentCityFavorite = result.Value;
                Logger.LogDebug($"Is {CurrentWeather.CityName} favorite: {IsCurrentCityFavorite}");
            }
            else
            {
                IsCurrentCityFavorite = false;
                Logger.LogWarning($"Failed to check favorite status: {result.Error?.Message}");
            }
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

    public void Dispose()
    {
        _settingsService.SettingsChanged -= OnSettingsChanged;
    }

    #endregion
}