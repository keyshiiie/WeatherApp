using CommunityToolkit.Mvvm.Input;
using WeatherApp.Core.Models;
using WeatherApp.Core.Services;

namespace WeatherApp.UI.ViewModels;

public partial class CurrentWeatherViewModel : BaseViewModel
{
    private readonly IWeatherService _weatherService;
    private readonly IFavoritesService _favoritesService;

    private WeatherData? _currentWeather;
    private List<ForecastDay>? _forecastDays;
    private City? _selectedCity;
    private bool _isRefreshing;
    private bool _isCurrentCityFavorite;


    public CurrentWeatherViewModel(
        IWeatherService weatherService,
        IFavoritesService favoritesService)
    {
        _weatherService = weatherService ?? throw new ArgumentNullException(nameof(weatherService));
        _favoritesService = favoritesService ?? throw new ArgumentNullException(nameof(favoritesService));

        Title = "Погода";

        RefreshCommand = new AsyncRelayCommand(RefreshWeatherAsync);
        GoBackCommand = new AsyncRelayCommand(GoBackAsync);
        AddToFavoritesCommand = new AsyncRelayCommand(AddCurrentCityToFavoritesAsync);
        RemoveFromFavoritesCommand = new AsyncRelayCommand(RemoveCurrentCityFromFavoritesAsync);
        ToggleFavoriteCommand = new AsyncRelayCommand(ToggleFavoriteAsync);
    }

    #region Properties

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
        if (SelectedCity != null && CurrentWeather == null)
        {
            await LoadWeatherForCityAsync(SelectedCity);
        }
    }

    public async Task LoadWeatherForCityAsync(City city)
    {
        if (city == null)
            return;

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

                await CheckIsFavoriteAsync();
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
            IsCurrentCityFavorite = await _favoritesService.IsFavoriteAsync(CurrentWeather.CityName);
        }
        else
        {
            IsCurrentCityFavorite = false;
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
                Latitude = CurrentWeather.Latitude,
                Longitude = CurrentWeather.Longitude,
                AddedAt = DateTime.UtcNow,
                IsLastSelected = false
            };

            await _favoritesService.AddFavoriteAsync(city);
            await CheckIsFavoriteAsync();
        }, "Не удалось добавить в избранное");
    }

    private async Task RemoveCurrentCityFromFavoritesAsync()
    {
        if (CurrentWeather == null || string.IsNullOrEmpty(CurrentWeather.CityName))
            return;

        await ExecuteAsync(async () =>
        {
            await _favoritesService.RemoveFavoriteByNameAsync(CurrentWeather.CityName);
            await CheckIsFavoriteAsync();
        }, "Не удалось удалить из избранного");
    }

    private async Task ToggleFavoriteAsync()
    {
        if (IsCurrentCityFavorite)
            await RemoveCurrentCityFromFavoritesAsync();
        else
            await AddCurrentCityToFavoritesAsync();
    }

    #endregion
}