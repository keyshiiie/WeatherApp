using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using WeatherApp.Core.Models;
using WeatherApp.Core.Services;
using WeatherApp.UI.DisplayModels;
using WeatherApp.UI.ViewModels;
using WeatherApp.UI.Views;

namespace WeatherApp.Core.ViewModels;

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
        ILogger<FavoritesPageViewModel> logger)
        : base(logger)
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

        await ExecuteAsync(async () =>
        {
            var freshList = await _cityService.GetFavoritesAsync() ?? new List<City>();

            Logger.LogInformation($"Loaded {freshList.Count} favorite cities");

            var settings = _settingsService.GetSettings();

            // Создаем Display модели
            FavoriteCities.Clear();
            foreach (var city in freshList)
            {
                var display = new FavoriteCityDisplay(city, settings);
                FavoriteCities.Add(display);
            }

            // Загружаем погоду для каждого города
            await LoadWeatherForAllCitiesAsync();
        }, "Не удалось загрузить избранное");
    }

    private async Task LoadWeatherForAllCitiesAsync()
    {
        var tasks = FavoriteCities.Select(city => LoadWeatherForCityAsync(city));
        await Task.WhenAll(tasks);
    }

    private async Task LoadWeatherForCityAsync(FavoriteCityDisplay display)
    {
        if (display?.City == null)
            return;

        try
        {
            // Устанавливаем состояние загрузки
            display.IsLoading = true;
            display.HasError = false;

            Logger.LogInformation($"Loading weather for {display.City.Name}");

            var weather = await _weatherService.GetCurrentWeatherAsync(
                display.City.Latitude,
                display.City.Longitude);

            if (weather != null)
            {
                // Обогащаем данные о городе
                weather.CityName = display.City.Name;
                weather.Country = display.City.Country;
                weather.Region = display.City.Region;

                display.Weather = weather;
                Logger.LogInformation($"Weather loaded for {display.City.Name}: {weather.TemperatureC}°C");
            }
            else
            {
                display.HasError = true;
                Logger.LogWarning($"Failed to load weather for {display.City.Name}");
            }
        }
        catch (Exception ex)
        {
            display.HasError = true;
            Logger.LogError(ex, $"Error loading weather for {display.City.Name}");
        }
        finally
        {
            // Сбрасываем состояние загрузки
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
            var cityJson = System.Text.Json.JsonSerializer.Serialize(city);
            var uri = $"{nameof(CurrentWeatherPage)}?city={Uri.EscapeDataString(cityJson)}";
            await Shell.Current.GoToAsync(uri);

            Logger.LogInformation($"Navigation to weather for {city.Name} successful");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error navigating to weather for {city.Name}");
            System.Diagnostics.Debug.WriteLine($"Ошибка навигации: {ex.Message}");
            SetError("Не удалось открыть страницу погоды");
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

        await ExecuteAsync(async () =>
        {
            await _cityService.RemoveFavoriteByNameAsync(city.Name!);

            var cityToRemove = FavoriteCities.FirstOrDefault(c => c.City.Name == city.Name);
            if (cityToRemove != null)
            {
                FavoriteCities.Remove(cityToRemove);
                Logger.LogInformation($"Removed {city.Name} from favorites");
            }
            else
            {
                Logger.LogWarning($"City {city.Name} not found in favorites list");
            }
        }, "Не удалось удалить город");
    }

    [RelayCommand]
    private async Task RefreshWeatherAsync()
    {
        Logger.LogInformation("Refreshing weather for favorites");
        await LoadWeatherForAllCitiesAsync();
    }
}