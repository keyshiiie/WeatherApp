using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using WeatherApp.Core.Models;
using WeatherApp.Core.Services;
using WeatherApp.UI.Views;
using static ToastService;

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
        Title = "Поиск";
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

        try
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

                await ToastService.ShowStatusToastAsync(
                    "Города не найдены. Попробуйте изменить запрос",
                    ToastType.Info
                );
            }
        }
        catch (Exception ex)
        {
            await ToastService.ShowStatusToastAsync(
                "Ошибка поиска городов. Проверьте подключение к интернету",
                ToastType.Error
            );
            Debug.WriteLine($"Search error: {ex.Message}");

            SearchSuggestions.Clear();
            ShowSearchSuggestions = false;
        }
    }

    [RelayCommand]
    private async Task SelectSuggestionAsync(CitySuggestion? suggestion)
    {
        if (suggestion == null)
            return;

        try
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

            await ToastService.ShowStatusToastAsync(
                $"Город {city.Name} добавлен в историю",
                ToastType.Success
            );
        }
        catch (Exception ex)
        {
            await ToastService.ShowStatusToastAsync(
                "Не удалось открыть погоду для выбранного города",
                ToastType.Error
            );
            Debug.WriteLine($"Select suggestion error: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task GetLocationAsync()
    {
        try
        {
            var hasPermission = await _geolocationService.RequestLocationPermissionAsync();
            if (!hasPermission)
            {
                await ToastService.ShowStatusToastAsync(
                    "Разрешение на определение местоположения не получено",
                    ToastType.Error
                );
                return;
            }

            var location = await _geolocationService.GetCurrentLocationAsync();
            if (location == null)
            {
                await ToastService.ShowStatusToastAsync(
                    "Не удалось определить местоположение",
                    ToastType.Error
                );
                return;
            }

            if (string.IsNullOrEmpty(location.Name))
            {
                await ToastService.ShowStatusToastAsync(
                    "Не удалось определить название города",
                    ToastType.Error
                );
                return;
            }

            location.Country ??= "Unknown";
            location.Region ??= "Unknown";

            await _cityService.AddInHistoryAsync(location);
            await NavigateToWeatherPage(location);

            await LoadCityListsAsync();

            await ToastService.ShowStatusToastAsync(
                $"Погода в {location.Name}",
                ToastType.Success
            );
        }
        catch (Exception ex)
        {
            await ToastService.ShowStatusToastAsync(
                "Не удалось определить местоположение",
                ToastType.Error
            );
            Debug.WriteLine($"Location error: {ex.Message}");
        }
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

        try
        {
            await NavigateToWeatherPage(city);
        }
        catch (Exception ex)
        {
            await ToastService.ShowStatusToastAsync(
                "Не удалось открыть погоду",
                ToastType.Error
            );
            Debug.WriteLine($"Select recent city error: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task RemoveRecentCityAsync(City? city)
    {
        if (city == null || string.IsNullOrWhiteSpace(city.Name)) return;

        try
        {
            await _cityService.RemoveFromHistoryByNameAsync(city.Name!);

            var cityToRemove = RecentCities.FirstOrDefault(c => c.Name == city.Name);
            if (cityToRemove != null)
            {
                RecentCities.Remove(cityToRemove);

                await ToastService.ShowStatusToastAsync(
                    $"Город {city.Name} удалён из истории",
                    ToastType.Info
                );
            }
        }
        catch (Exception ex)
        {
            await ToastService.ShowStatusToastAsync(
                $"Не удалось удалить город {city.Name}",
                ToastType.Error
            );
            Debug.WriteLine($"Remove city error: {ex.Message}");
        }
    }

    #endregion

    #region Private Methods

    private async Task LoadCityListsAsync()
    {
        try
        {
            var history = await _cityService.GetHistoryAsync() ?? new List<City>();

            RecentCities.Clear();
            foreach (var city in history)
            {
                RecentCities.Add(city);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Load history error: {ex.Message}");
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
            Debug.WriteLine($"Navigation error: {ex.Message}");
            throw;
        }
    }

    #endregion
}