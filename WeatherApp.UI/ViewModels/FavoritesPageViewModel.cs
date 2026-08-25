using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using WeatherApp.Core.Models;
using WeatherApp.Core.Results;
using WeatherApp.Core.Services;
using WeatherApp.UI.DisplayModels;
using WeatherApp.UI.Services;
using WeatherApp.UI.Views;

namespace WeatherApp.UI.ViewModels;

public partial class FavoritesPageViewModel : BaseViewModel
{
    private readonly ICityService _cityService;
    private readonly IWeatherService _weatherService;
    private readonly ISettingsService _settingsService;

    [ObservableProperty]
    private ObservableCollection<FavoriteCityDisplay> _favoriteCities = new();

    public FavoritesPageViewModel(
        ICityService cityService,
        IWeatherService weatherService,
        ISettingsService settingsService,
        INavigationService navigationService,
        ILogger<FavoritesPageViewModel> logger)
        : base(logger, navigationService)
    {
        _cityService = cityService ?? throw new ArgumentNullException(nameof(cityService));
        _weatherService = weatherService ?? throw new ArgumentNullException(nameof(weatherService));
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));

        Title = "Избранное";

        Logger.LogInformation("FavoritesPageViewModel initialized");
    }

    public override async Task OnAppearingAsync()
    {
        Logger.LogInformation("Favorites page appearing");
        await LoadFavoritesAsync();
    }

    [RelayCommand]
    public async Task LoadFavoritesAsync()
    {
        Logger.LogInformation("Loading favorites");

        var result = await ExecuteWithResultAsync(
            async () =>
            {
                var favoritesResult = await _cityService.GetFavoritesAsync();
                if (favoritesResult.IsFailure)
                    return Result.Failure<List<City>>(favoritesResult.Error!);

                var freshList = favoritesResult.Value ?? new List<City>();
                Logger.LogInformation($"Loaded {freshList.Count} favorite cities");

                var settingsResult = _settingsService.GetSettings();
                var settings = settingsResult.IsSuccess ? settingsResult.Value! : new UserSettings();

                FavoriteCities.Clear();
                foreach (var city in freshList)
                {
                    var display = new FavoriteCityDisplay(city, settings);
                    FavoriteCities.Add(display);
                }

                await LoadWeatherForAllCitiesAsync();

                return Result.Success(freshList);
            },
            errorMessage: "Не удалось загрузить избранное"
        );
    }

    private async Task LoadWeatherForAllCitiesAsync()
    {
        // Ограничиваем количество параллельных запросов
        var semaphore = new SemaphoreSlim(3);
        var tasks = FavoriteCities.Select(async city =>
        {
            await semaphore.WaitAsync();
            try
            {
                await LoadWeatherForCityAsync(city);
            }
            finally
            {
                semaphore.Release();
            }
        });
        await Task.WhenAll(tasks);
    }

    private async Task LoadWeatherForCityAsync(FavoriteCityDisplay display)
    {
        if (display?.City == null)
            return;

        try
        {
            display.IsLoading = true;
            display.HasError = false;

            Logger.LogInformation($"Loading weather for {display.City.Name}");

            var weatherResult = await _weatherService.GetCurrentWeatherAsync(
                display.City.Latitude,
                display.City.Longitude);

            if (weatherResult.IsSuccess && weatherResult.Value != null)
            {
                var weather = weatherResult.Value;
                weather.CityName = display.City.Name;
                weather.Country = display.City.Country;
                weather.Region = display.City.Region;

                display.Weather = weather;
                Logger.LogInformation($"Weather loaded for {display.City.Name}: {weather.TemperatureC}°C");
            }
            else
            {
                display.HasError = true;
                Logger.LogWarning($"Failed to load weather for {display.City.Name}: {weatherResult.Error?.Message}");
            }
        }
        catch (Exception ex)
        {
            display.HasError = true;
            Logger.LogError(ex, $"Error loading weather for {display.City.Name}");
        }
        finally
        {
            display.IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task GoToWeatherAsync(FavoriteCityDisplay display)
    {
        if (display?.City == null)
        {
            Logger.LogWarning("GoToWeatherAsync called with null city");
            return;
        }

        var city = display.City;
        Logger.LogInformation($"Navigating to weather for favorite city: {city.Name}");

        try
        {
            await NavigationService.GoToWeatherPageAsync(city);
            Logger.LogInformation($"Navigation to weather for {city.Name} successful");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error navigating to weather for {city.Name}");
            await ShowAlertAsync("Ошибка", "Не удалось открыть страницу погоды");
        }
    }

    [RelayCommand]
    public async Task RemoveFavoriteAsync(FavoriteCityDisplay display)
    {
        if (display?.City == null || string.IsNullOrWhiteSpace(display.City.Name))
        {
            Logger.LogWarning("RemoveFavoriteAsync called with null city or empty name");
            return;
        }

        var city = display.City;
        Logger.LogInformation($"Removing {city.Name} from favorites");

        var result = await ExecuteWithResultAsync(
            async () =>
            {
                var removeResult = await _cityService.RemoveFavoriteAsync(city.Id);
                if (removeResult.IsFailure)
                    return Result.Failure(removeResult.Error!);

                var cityToRemove = FavoriteCities.FirstOrDefault(c => c.City.Name == city.Name);
                if (cityToRemove != null)
                {
                    FavoriteCities.Remove(cityToRemove);
                    Logger.LogInformation($"Removed {city.Name} from favorites");
                }

                return Result.Success();
            },
            successMessage: $"{city.Name} удален из избранного",
            errorMessage: "Не удалось удалить город"
        );
    }

    [RelayCommand]
    private async Task RefreshWeatherAsync()
    {
        Logger.LogInformation("Refreshing weather for favorites");
        await LoadWeatherForAllCitiesAsync();
    }
}