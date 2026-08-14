using WeatherApp.Core.Models;
using WeatherApp.Core.Repositories;

namespace WeatherApp.Core.Services;

public class CityService : ICityService
{
    private readonly IFavoritesRepository _favorites;
    private readonly IHistoryRepository _history;

    public CityService(IFavoritesRepository favorites, IHistoryRepository history)
    {
        _favorites = favorites ?? throw new ArgumentNullException(nameof(favorites));
        _history = history ?? throw new ArgumentNullException(nameof(history));
    }

    public Task<List<City>> GetFavoritesAsync(CancellationToken cancellationToken = default)
        => _favorites.GetFavoritesAsync(cancellationToken);

    public Task<City> AddFavoriteAsync(City city, CancellationToken cancellationToken = default)
        => _favorites.AddFavoriteAsync(city, cancellationToken);

    public Task<bool> RemoveFavoriteAsync(int cityId, CancellationToken cancellationToken = default)
        => _favorites.RemoveFavoriteAsync(cityId, cancellationToken);

    public Task<bool> RemoveFavoriteByNameAsync(string cityName, CancellationToken cancellationToken = default)
        => _favorites.RemoveFavoriteByNameAsync(cityName, cancellationToken);

    public Task<City?> GetLastFavoriteAsync(CancellationToken cancellationToken = default)
        => _favorites.GetLastFavoriteAsync(cancellationToken);

    public Task SetLastFavoriteAsync(City city, CancellationToken cancellationToken = default)
        => _favorites.SetLastFavoriteAsync(city, cancellationToken);

    public Task<bool> IsFavoriteAsync(string cityName, CancellationToken cancellationToken = default)
        => _favorites.IsFavoriteAsync(cityName, cancellationToken);

    public Task<int> GetFavoritesCountAsync(CancellationToken cancellationToken = default)
        => _favorites.GetFavoritesCountAsync(cancellationToken);

    public Task ClearAllFavoritesAsync(CancellationToken cancellationToken = default)
        => _favorites.ClearAllFavoritesAsync(cancellationToken);

    public Task<List<City>> GetHistoryAsync(CancellationToken cancellationToken = default)
        => _history.GetHistoryAsync(cancellationToken);

    public Task<City> AddInHistoryAsync(City city, CancellationToken cancellationToken = default)
        => _history.AddInHistoryAsync(city, cancellationToken);

    public Task<bool> RemoveFromHistoryAsync(int cityId, CancellationToken cancellationToken = default)
        => _history.RemoveFromHistoryAsync(cityId, cancellationToken);

    public Task<bool> RemoveFromHistoryByNameAsync(string cityName, CancellationToken cancellationToken = default)
        => _history.RemoveFromHistoryByNameAsync(cityName, cancellationToken);

    public Task<bool> IsRecentAsync(string cityName, CancellationToken cancellationToken = default)
        => _history.IsRecentAsync(cityName, cancellationToken);

    public Task<int> GetHistoryCountAsync(CancellationToken cancellationToken = default)
        => _history.GetHistoryCountAsync(cancellationToken);

    public Task ClearHistoryAsync(CancellationToken cancellationToken = default)
        => _history.ClearHistoryAsync(cancellationToken);

    public async Task<City?> GetBestCityAsync(CancellationToken cancellationToken = default)
    {
        var favorites = await _favorites.GetFavoritesAsync(cancellationToken);
        if (favorites != null && favorites.Any())
        {
            return favorites.OrderByDescending(c => c.AddedAt).First();
        }

        var history = await _history.GetHistoryAsync(cancellationToken);
        if (history != null && history.Any())
        {
            return history.First(); 
        }

        return null;
    }
}