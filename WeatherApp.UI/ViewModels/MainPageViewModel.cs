using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using WeatherApp.Core.Models;
using WeatherApp.Core.Results;
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

        var result = await ExecuteWithResultAsync(
            async () => await _weatherService.SearchCitiesAsync(SearchQuery),
            errorMessage: "Ошибка поиска городов"
        );

        if (result.IsSuccess && result.Value != null)
        {
            if (result.Value.Any())
            {
                Logger.LogInformation($"Found {result.Value.Count} cities for '{SearchQuery}'");
                SearchSuggestions = result.Value.Take(10).ToList();
                ShowSearchSuggestions = SearchSuggestions.Any();
            }
            else
            {
                Logger.LogWarning($"No cities found for '{SearchQuery}'");
                SearchSuggestions.Clear();
                ShowSearchSuggestions = false;
                await ShowToastAsync("Города не найдены. Попробуйте изменить запрос");
            }
        }
        else
        {
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

        var result = await ExecuteWithResultAsync<City>(
            async () =>
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

                var addResult = await _cityService.AddInHistoryAsync(city);
                if (addResult.IsFailure)
                    return Result.Failure<City>(addResult.Error!);

                Logger.LogInformation($"City added to history: {city.Name}");
                return Result.Success(city);
            },
            successMessage: $"{suggestion.Name} добавлен в историю",
            errorMessage: "Не удалось добавить город в историю"
        );

        if (result.IsSuccess)
        {
            await NavigateToWeatherPage(result.Value!);

            SearchQuery = string.Empty;
            SearchSuggestions.Clear();
            ShowSearchSuggestions = false;

            await LoadCityListsAsync();
        }
    }

    [RelayCommand]
    private async Task GetLocationAsync()
    {
        Logger.LogInformation("Getting current location");

        var result = await ExecuteWithResultAsync<City>(
            async () =>
            {
                var hasPermission = await _geolocationService.RequestLocationPermissionAsync();
                if (!hasPermission)
                {
                    Logger.LogWarning("Location permission denied");
                    return Result.Failure<City>(new ValidationError("Разрешение на определение местоположения не получено"));
                }

                var location = await _geolocationService.GetCurrentLocationAsync();
                if (location == null)
                {
                    Logger.LogWarning("Location is null");
                    return Result.Failure<City>(new NotFoundError("Location", "current"));
                }

                if (string.IsNullOrEmpty(location.Name))
                {
                    Logger.LogWarning("Location name is empty");
                    return Result.Failure<City>(new ValidationError("Не удалось определить название города"));
                }

                Logger.LogInformation($"Location found: {location.Name}, {location.Country}");

                location.Country ??= "Unknown";
                location.Region ??= "Unknown";

                var addResult = await _cityService.AddInHistoryAsync(location);
                if (addResult.IsFailure)
                    return Result.Failure<City>(addResult.Error!);

                Logger.LogInformation($"Location added to history: {location.Name}");
                return Result.Success(location);
            },
            successMessage: $"Определено местоположение",
            errorMessage: "Не удалось определить местоположение"
        );

        if (result.IsSuccess)
        {
            await NavigateToWeatherPage(result.Value!);
            await LoadCityListsAsync();
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
        await NavigateToWeatherPage(city);
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

        var result = await ExecuteWithResultAsync(
            async () =>
            {
                var removeResult = await _cityService.RemoveFromHistoryAsync(city.Id);
                if (removeResult.IsFailure)
                    return Result.Failure(removeResult.Error!);

                Logger.LogInformation($"City removed from history: {city.Name}");
                return Result.Success();
            },
            successMessage: $"{city.Name} удален из истории",
            errorMessage: $"Не удалось удалить город {city.Name}"
        );

        if (result.IsSuccess)
        {
            var cityToRemove = RecentCities.FirstOrDefault(c => c.Name == city.Name);
            if (cityToRemove != null)
            {
                RecentCities.Remove(cityToRemove);
            }
        }
    }

    #endregion

    #region Private Methods

    private async Task LoadCityListsAsync()
    {
        Logger.LogInformation("Loading city lists");

        var result = await ExecuteWithResultAsync(
            async () =>
            {
                var historyResult = await _cityService.GetHistoryAsync();
                if (historyResult.IsFailure)
                    return Result.Failure<List<City>>(historyResult.Error!);

                Logger.LogInformation($"Loaded {historyResult.Value?.Count ?? 0} cities from history");
                return Result.Success(historyResult.Value ?? new List<City>());
            },
            errorMessage: "Не удалось загрузить историю"
        );

        if (result.IsSuccess)
        {
            RecentCities.Clear();
            foreach (var city in result.Value!)
            {
                RecentCities.Add(city);
            }
            ShowRecent = RecentCities.Any();
        }
        else
        {
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
            await ShowAlertAsync("Ошибка", $"Не удалось открыть страницу погоды для {city.Name}");
        }
    }

    #endregion
}