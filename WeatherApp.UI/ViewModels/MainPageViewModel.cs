using CommunityToolkit.Mvvm.Input;
using WeatherApp.Core.Models;
using WeatherApp.Core.Services;
using WeatherApp.UI.Views;

namespace WeatherApp.UI.ViewModels;

public partial class MainPageViewModel : BaseViewModel
{
    private readonly IWeatherService _weatherService;
    private readonly IGeolocationService _geolocationService;
    private readonly IFavoritesService _favoritesService;

    private string _searchQuery = string.Empty;
    private List<CitySuggestion> _searchSuggestions = new();
    private bool _showSearchSuggestions;

    public MainPageViewModel(
        IWeatherService weatherService,
        IGeolocationService geolocationService,
        IFavoritesService favoritesService)
    {
        _weatherService = weatherService ?? throw new ArgumentNullException(nameof(weatherService));
        _geolocationService = geolocationService ?? throw new ArgumentNullException(nameof(geolocationService));
        _favoritesService = favoritesService ?? throw new ArgumentNullException(nameof(favoritesService));

        Title = "Поиск города";

        SearchCommand = new AsyncRelayCommand(SearchCitiesAsync);
        SelectSuggestionCommand = new AsyncRelayCommand<CitySuggestion>(OnSelectSuggestionAsync);
        GetLocationCommand = new AsyncRelayCommand(GetLocationAsync);
        ClearSearchCommand = new RelayCommand(ClearSearch);
    }

    #region Properties

    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (SetProperty(ref _searchQuery, value))
            {
                if (!string.IsNullOrWhiteSpace(value) && value.Length >= 2)
                {
                    SearchCommand.Execute(null);
                }
                else
                {
                    SearchSuggestions.Clear();
                    ShowSearchSuggestions = false;
                }
            }
        }
    }

    public List<CitySuggestion> SearchSuggestions
    {
        get => _searchSuggestions;
        set => SetProperty(ref _searchSuggestions, value);
    }

    public bool ShowSearchSuggestions
    {
        get => _showSearchSuggestions;
        set => SetProperty(ref _showSearchSuggestions, value);
    }

    #endregion

    #region Commands

    public IAsyncRelayCommand SearchCommand { get; }
    public IAsyncRelayCommand<CitySuggestion> SelectSuggestionCommand { get; }
    public IAsyncRelayCommand GetLocationCommand { get; }
    public IRelayCommand ClearSearchCommand { get; }

    #endregion

    #region Private Methods

    private async Task SearchCitiesAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery) || SearchQuery.Length < 2)
        {
            SearchSuggestions.Clear();
            ShowSearchSuggestions = false;
            return;
        }

        await ExecuteAsync(async () =>
        {
            var results = await _weatherService.SearchCitiesAsync(SearchQuery);
            if (results != null && results.Any())
            {
                SearchSuggestions = results.Take(10).ToList();
                ShowSearchSuggestions = SearchSuggestions.Any();
            }
            else
            {
                SearchSuggestions.Clear();
                ShowSearchSuggestions = false;
            }
        }, "Ошибка поиска городов");
    }

    private async Task OnSelectSuggestionAsync(CitySuggestion? suggestion)
    {
        if (suggestion == null)
            return;

        await ExecuteAsync(async () =>
        {
            // Создаем город из предложения
            var city = new City
            {
                Name = suggestion.Name,
                Country = suggestion.Country,
                Region = suggestion.Region,
                Latitude = suggestion.Latitude, 
                Longitude = suggestion.Longitude,
                AddedAt = DateTime.UtcNow,
                IsLastSelected = false
            };

            System.Diagnostics.Debug.WriteLine($"🏙️ Выбран город: {city.Name}, {city.Country}");
            System.Diagnostics.Debug.WriteLine($"📍 Точные координаты: Lat={city.Latitude}, Lon={city.Longitude}");

            // Переходим на страницу погоды
            await NavigateToWeatherPage(city);

            // Очищаем поиск
            SearchQuery = string.Empty;
            SearchSuggestions.Clear();
            ShowSearchSuggestions = false;
        }, "Не удалось открыть погоду");
    }

    private async Task GetLocationAsync()
    {
        await ExecuteAsync(async () =>
        {
            var hasPermission = await _geolocationService.RequestLocationPermissionAsync();
            if (!hasPermission)
            {
                SetError("Не удалось получить разрешение на определение местоположения.");
                return;
            }

            var location = await _geolocationService.GetCurrentLocationAsync();
            if (location == null)
            {
                SetError("Не удалось определить местоположение.");
                return;
            }

            if (string.IsNullOrEmpty(location.Name))
            {
                SetError("Не удалось определить название города.");
                return;
            }

            location.Country ??= "Unknown";
            location.Region ??= "Unknown";

            System.Diagnostics.Debug.WriteLine($"📍 Определен город: {location.DisplayName}");
            System.Diagnostics.Debug.WriteLine($"📍 Координаты: {location.Latitude}, {location.Longitude}");

            await NavigateToWeatherPage(location);
        }, "Не удалось определить местоположение");
    }

    private async Task NavigateToWeatherPage(City city)
    {
        try
        {
            var cityJson = System.Text.Json.JsonSerializer.Serialize(city);

            var uri = $"{nameof(CurrentWeatherPage)}?city={Uri.EscapeDataString(cityJson)}";

            await Shell.Current.GoToAsync(uri);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка навигации: {ex.Message}");
            SetError($"Не удалось открыть страницу погоды: {ex.Message}");
        }
    }

    private void ClearSearch()
    {
        SearchQuery = string.Empty;
        SearchSuggestions.Clear();
        ShowSearchSuggestions = false;
    }

    #endregion
}