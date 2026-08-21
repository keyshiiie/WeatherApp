using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
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

    [ObservableProperty]
    private bool _showRecent;

    [ObservableProperty]
    private bool _isBusy;

    public MainPageViewModel(
        IWeatherService weatherService,
        IGeolocationService geolocationService,
        ICityService cityService,
        ILogger<MainPageViewModel> logger) 
        : base(logger)
    {
        Title = "Поиск";
        _weatherService = weatherService ?? throw new ArgumentNullException(nameof(weatherService));
        _geolocationService = geolocationService ?? throw new ArgumentNullException(nameof(geolocationService));
        _cityService = cityService ?? throw new ArgumentNullException(nameof(cityService));
    }

    public override async Task OnAppearingAsync()
    {
        Logger.LogInformation("MainPage appearing");
        await LoadCityListsAsync();
    }

    partial void OnSearchQueryChanged(string value)
    {
        Logger.LogDebug($"Search query changed: {value}");

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

        Logger.LogInformation($"Searching cities: {SearchQuery}");

        try
        {
            var results = await _weatherService.SearchCitiesAsync(SearchQuery);

            if (results != null && results.Any())
            {
                Logger.LogInformation($"Found {results.Count()} cities for '{SearchQuery}'");
                SearchSuggestions = results.Take(10).ToList();
                ShowSearchSuggestions = SearchSuggestions.Any();
            }
            else
            {
                Logger.LogWarning($"No cities found for '{SearchQuery}'");
                SearchSuggestions.Clear();
                ShowSearchSuggestions = false;

                await Toast.Make("Города не найдены. Попробуйте изменить запрос", ToastDuration.Long).Show();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error searching cities: {SearchQuery}");
            await Toast.Make("Ошибка поиска городов. Проверьте подключение к интернету", ToastDuration.Long).Show();

            SearchSuggestions.Clear();
            ShowSearchSuggestions = false;
        }
    }

    [RelayCommand]
    private async Task SelectSuggestionAsync(CitySuggestion? suggestion)
    {
        if (suggestion == null)
        {
            Logger.LogWarning("Suggestion is null");
            return;
        }

        Logger.LogInformation($"Selecting city: {suggestion.Name}, {suggestion.Country}");

        try
        {
            IsBusy = true;

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
            Logger.LogInformation($"City added to history: {city.Name}");

            await NavigateToWeatherPage(city);

            SearchQuery = string.Empty;
            SearchSuggestions.Clear();
            ShowSearchSuggestions = false;

            await LoadCityListsAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error selecting city: {suggestion.Name}");
            await Toast.Make("Не удалось открыть погоду для выбранного города", ToastDuration.Long).Show();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GetLocationAsync()
    {
        Logger.LogInformation("Getting current location");

        try
        {
            IsBusy = true;

            var hasPermission = await _geolocationService.RequestLocationPermissionAsync();
            if (!hasPermission)
            {
                Logger.LogWarning("Location permission denied");
                await Toast.Make("Разрешение на определение местоположения не получено", ToastDuration.Long).Show();
                return;
            }

            var location = await _geolocationService.GetCurrentLocationAsync();
            if (location == null)
            {
                Logger.LogWarning("Location is null");
                await Toast.Make("Не удалось определить местоположение", ToastDuration.Long).Show();
                return;
            }

            if (string.IsNullOrEmpty(location.Name))
            {
                Logger.LogWarning("Location name is empty");
                await Toast.Make("Не удалось определить название города", ToastDuration.Long).Show();
                return;
            }

            Logger.LogInformation($"Location found: {location.Name}, {location.Country}");

            location.Country ??= "Unknown";
            location.Region ??= "Unknown";

            await _cityService.AddInHistoryAsync(location);
            Logger.LogInformation($"Location added to history: {location.Name}");

            await NavigateToWeatherPage(location);
            await LoadCityListsAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting location");
            await Toast.Make("Не удалось определить местоположение", ToastDuration.Long).Show();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ClearSearch()
    {
        Logger.LogDebug("Clearing search");
        SearchQuery = string.Empty;
        SearchSuggestions.Clear();
        ShowSearchSuggestions = false;
    }

    [RelayCommand]
    private async Task SelectRecentCityAsync(City? city)
    {
        if (city == null)
        {
            Logger.LogWarning("Recent city is null");
            return;
        }

        Logger.LogInformation($"Selecting recent city: {city.Name}");

        try
        {
            await NavigateToWeatherPage(city);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error selecting recent city: {city.Name}");
            await Toast.Make("Не удалось открыть погоду", ToastDuration.Long).Show();
        }
    }

    [RelayCommand]
    private async Task RemoveRecentCityAsync(City? city)
    {
        if (city == null || string.IsNullOrWhiteSpace(city.Name))
        {
            Logger.LogWarning("Cannot remove city: null or empty name");
            return;
        }

        Logger.LogInformation($"Removing city from history: {city.Name}");

        try
        {
            await _cityService.RemoveFromHistoryByNameAsync(city.Name!);

            var cityToRemove = RecentCities.FirstOrDefault(c => c.Name == city.Name);
            if (cityToRemove != null)
            {
                RecentCities.Remove(cityToRemove);
                Logger.LogInformation($"City removed from history: {city.Name}");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error removing city from history: {city.Name}");
            await Toast.Make($"Не удалось удалить город {city.Name}", ToastDuration.Long).Show();
        }
    }

    #endregion

    #region Private Methods

    private async Task LoadCityListsAsync()
    {
        Logger.LogInformation("Loading city lists");

        try
        {
            var history = await _cityService.GetHistoryAsync() ?? new List<City>();

            Logger.LogInformation($"Loaded {history.Count} cities from history");

            RecentCities.Clear();
            foreach (var city in history)
            {
                RecentCities.Add(city);
            }

            ShowRecent = RecentCities.Any();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading city lists");
            ShowRecent = false;
        }
    }

    private async Task NavigateToWeatherPage(City city)
    {
        try
        {
            Logger.LogInformation($"Navigating to weather page for: {city.Name}");

            var cityJson = System.Text.Json.JsonSerializer.Serialize(city);
            var uri = $"{nameof(CurrentWeatherPage)}?city={Uri.EscapeDataString(cityJson)}";
            await Shell.Current.GoToAsync(uri);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Navigation error for city: {city.Name}");
            throw;
        }
    }

    #endregion
}