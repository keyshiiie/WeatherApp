using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using WeatherApp.Core.Models;
using WeatherApp.Core.Services;
using WeatherApp.UI.Views;

namespace WeatherApp.UI.ViewModels;

public partial class MainPageViewModel : BaseViewModel
{
    private readonly IWeatherService _weatherService;
    private readonly IGeolocationService _geolocationService;
    private readonly ICityService _cityService;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private ObservableCollection<City> _recentCities = new();

    [ObservableProperty]
    private List<CitySuggestion> _searchSuggestions = new();

    [ObservableProperty]
    private bool _showSearchSuggestions;

    public MainPageViewModel(
        IWeatherService weatherService,
        IGeolocationService geolocationService,
        ICityService cityService)
    {
        _weatherService = weatherService ?? throw new ArgumentNullException(nameof(weatherService));
        _geolocationService = geolocationService ?? throw new ArgumentNullException(nameof(geolocationService));
        _cityService = cityService ?? throw new ArgumentNullException(nameof(cityService));
    }

    public override async Task OnAppearingAsync()
    {
        await LoadCityListsAsync();
    }

    partial void OnSearchQueryChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value) && value.Length >= 2)
        {
            SearchCitiesCommand.Execute(null);
        }
        else
        {
            SearchSuggestions.Clear();
            ShowSearchSuggestions = false;
        }
    }

    #region Commands

    [RelayCommand]
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

    [RelayCommand]
    private async Task SelectSuggestionAsync(CitySuggestion? suggestion)
    {
        if (suggestion == null)
            return;

        await ExecuteAsync(async () =>
        {
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

            await _cityService.AddInHistoryAsync(city);
            await NavigateToWeatherPage(city);

            SearchQuery = string.Empty;
            SearchSuggestions.Clear();
            ShowSearchSuggestions = false;

            await LoadCityListsAsync();
        }, "Не удалось открыть погоду");
    }

    [RelayCommand]
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

            await _cityService.AddInHistoryAsync(location);
            await NavigateToWeatherPage(location);

            await LoadCityListsAsync();
        }, "Не удалось определить местоположение");
    }

    [RelayCommand]
    private void ClearSearch()
    {
        SearchQuery = string.Empty;
        SearchSuggestions.Clear();
        ShowSearchSuggestions = false;
    }

    [RelayCommand]
    private async Task SelectRecentCityAsync(City? city)
    {
        if (city == null) return;
        await NavigateToWeatherPage(city);
    }

    [RelayCommand]
    private async Task RemoveRecentCityAsync(City? city)
    {
        if (city == null || string.IsNullOrWhiteSpace(city.Name)) return;

        await ExecuteAsync(async () =>
        {
            await _cityService.RemoveFromHistoryByNameAsync(city.Name!);

            var cityToRemove = RecentCities.FirstOrDefault(c => c.Name == city.Name);
            if (cityToRemove != null)
            {
                RecentCities.Remove(cityToRemove);
            }
        }, "Не удалось удалить город");
    }

    #endregion

    #region Private Methods

    private async Task LoadCityListsAsync()
    {
        var history = await _cityService.GetHistoryAsync() ?? new List<City>();

        RecentCities.Clear();
        foreach (var city in history)
        {
            RecentCities.Add(city);
        }
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

    #endregion
}