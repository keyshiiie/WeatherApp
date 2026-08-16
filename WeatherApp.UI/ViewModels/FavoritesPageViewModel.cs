using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using WeatherApp.Core.Models;
using WeatherApp.Core.Services;
using WeatherApp.UI.ViewModels;
using WeatherApp.UI.Views;

namespace WeatherApp.Core.ViewModels;

public partial class FavoritesPageViewModel : BaseViewModel
{
    private readonly ICityService _cityService;

    private ObservableCollection<City> _favoriteCities = new();
    public ObservableCollection<City> FavoriteCities
    {
        get => _favoriteCities;
        set => SetProperty(ref _favoriteCities, value);
    }

    public FavoritesPageViewModel(
        ICityService cityService,
        ILogger<FavoritesPageViewModel> logger) // Добавляем логгер
        : base(logger) // Передаем в базовый класс
    {
        _cityService = cityService ?? throw new ArgumentNullException(nameof(cityService));
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

            FavoriteCities.Clear();
            foreach (var city in freshList)
            {
                FavoriteCities.Add(city);
            }
        }, "Не удалось загрузить избранное");
    }

    [RelayCommand]
    public async Task GoToWeatherAsync(City city)
    {
        if (city == null)
        {
            Logger.LogWarning("GoToWeatherAsync called with null city");
            return;
        }

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
    public async Task RemoveFavoriteAsync(City city)
    {
        if (city == null || string.IsNullOrWhiteSpace(city.Name))
        {
            Logger.LogWarning("RemoveFavoriteAsync called with null city or empty name");
            return;
        }

        Logger.LogInformation($"Removing {city.Name} from favorites");

        await ExecuteAsync(async () =>
        {
            await _cityService.RemoveFavoriteByNameAsync(city.Name!);

            var cityToRemove = FavoriteCities.FirstOrDefault(c => c.Name == city.Name);
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
}