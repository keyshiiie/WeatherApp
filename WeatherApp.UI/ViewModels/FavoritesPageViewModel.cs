using CommunityToolkit.Mvvm.Input;
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

    public FavoritesPageViewModel(ICityService cityService)
    {
        _cityService = cityService ?? throw new ArgumentNullException(nameof(cityService));
        Title = "Избранное";
    }

    [RelayCommand]
    public async Task LoadFavoritesAsync()
    {
        await ExecuteAsync(async () =>
        {
            var freshList = await _cityService.GetFavoritesAsync() ?? new List<City>();

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
        if (city == null) return;

        try
        {
            var cityJson = System.Text.Json.JsonSerializer.Serialize(city);
            var uri = $"{nameof(CurrentWeatherPage)}?city={Uri.EscapeDataString(cityJson)}";
            await Shell.Current.GoToAsync(uri);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка навигации: {ex.Message}");
            SetError("Не удалось открыть страницу погоды");
        }
    }

    [RelayCommand]
    public async Task RemoveFavoriteAsync(City city)
    {
        if (city == null || string.IsNullOrWhiteSpace(city.Name)) return;

        await ExecuteAsync(async () =>
        {
            await _cityService.RemoveFavoriteByNameAsync(city.Name!);

            var cityToRemove = FavoriteCities.FirstOrDefault(c => c.Name == city.Name);
            if (cityToRemove != null)
            {
                FavoriteCities.Remove(cityToRemove);
            }
        }, "Не удалось удалить город");
    }
}