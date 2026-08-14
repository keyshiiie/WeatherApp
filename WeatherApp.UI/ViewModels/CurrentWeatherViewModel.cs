using CommunityToolkit.Mvvm.Input;
using WeatherApp.Core.Models;
using WeatherApp.Core.Services;

namespace WeatherApp.UI.ViewModels;

public partial class CurrentWeatherViewModel : BaseViewModel
{
    private readonly IWeatherService _weatherService;
    private readonly ICityService _cityService;

    private WeatherData? _currentWeather;
    private List<ForecastDay>? _forecastDays;
    private City? _selectedCity;
    private bool _isRefreshing;
    private bool _isCurrentCityFavorite;


    public CurrentWeatherViewModel(
        IWeatherService weatherService,
        ICityService cityService)
    {
        _weatherService = weatherService ?? throw new ArgumentNullException(nameof(weatherService));
        _cityService = cityService ?? throw new ArgumentNullException(nameof(cityService));

        Title = "Погода";

        RefreshCommand = new AsyncRelayCommand(RefreshWeatherAsync);
        GoBackCommand = new AsyncRelayCommand(GoBackAsync);
        AddToFavoritesCommand = new AsyncRelayCommand(AddCurrentCityToFavoritesAsync);
        RemoveFromFavoritesCommand = new AsyncRelayCommand(RemoveCurrentCityFromFavoritesAsync);
        ToggleFavoriteCommand = new AsyncRelayCommand(ToggleFavoriteAsync);
    }
    #region Properties

    private TemperatureGraphDrawable _temperatureGraphDrawable = new();
    public TemperatureGraphDrawable TemperatureGraphDrawable
    {
        get => _temperatureGraphDrawable;
        set => SetProperty(ref _temperatureGraphDrawable, value);
    }

    private List<HourlyForecast> _hourlyForecast = new();
    public List<HourlyForecast> HourlyForecast
    {
        get => _hourlyForecast;
        set => SetProperty(ref _hourlyForecast, value);
    }

    public WeatherData? CurrentWeather
    {
        get => _currentWeather;
        set => SetProperty(ref _currentWeather, value);
    }

    public List<ForecastDay>? ForecastDays
    {
        get => _forecastDays;
        set => SetProperty(ref _forecastDays, value);
    }

    public City? SelectedCity
    {
        get => _selectedCity;
        set => SetProperty(ref _selectedCity, value);
    }

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set => SetProperty(ref _isRefreshing, value);
    }

    public bool IsCurrentCityFavorite
    {
        get => _isCurrentCityFavorite;
        private set => SetProperty(ref _isCurrentCityFavorite, value);
    }

    #endregion

    #region Commands

    public IAsyncRelayCommand RefreshCommand { get; }
    public IAsyncRelayCommand GoBackCommand { get; }
    public IAsyncRelayCommand AddToFavoritesCommand { get; }
    public IAsyncRelayCommand RemoveFromFavoritesCommand { get; }
    public IAsyncRelayCommand ToggleFavoriteCommand { get; }

    #endregion

    #region Public Methods

    public override async Task OnAppearingAsync()
    {
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
            return;

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

                    TemperatureGraphDrawable.Data = HourlyForecast;
                    OnPropertyChanged(nameof(TemperatureGraphDrawable));
                }
                else
                {
                    HourlyForecast = new List<HourlyForecast>();
                }

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

    private async Task RefreshWeatherAsync()
    {
        if (IsRefreshing || SelectedCity == null)
            return;

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

    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    private async Task AddCurrentCityToFavoritesAsync()
    {
        if (CurrentWeather == null || string.IsNullOrEmpty(CurrentWeather.CityName))
            return;

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

    private async Task RemoveCurrentCityFromFavoritesAsync()
    {
        if (CurrentWeather == null || string.IsNullOrEmpty(CurrentWeather.CityName))
            return;

        await ExecuteAsync(async () =>
        {
            await _cityService.RemoveFavoriteByNameAsync(CurrentWeather.CityName);

            await CheckIsFavoriteAsync();

            UpdateFavoriteToolbarItem();
        }, "Не удалось удалить из избранного");
    }

    private async Task ToggleFavoriteAsync()
    {
        if (IsCurrentCityFavorite)
            await RemoveCurrentCityFromFavoritesAsync();
        else
            await AddCurrentCityToFavoritesAsync();
    }

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