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

    public Task<bool> IsFavoriteAsync(string cityName, CancellationToken cancellationToken = default)
        => _favorites.IsFavoriteAsync(cityName, cancellationToken);

    public Task ClearAllFavoritesAsync(CancellationToken cancellationToken = default)
        => _favorites.ClearAllFavoritesAsync(cancellationToken);

    public Task<List<City>> GetHistoryAsync(CancellationToken cancellationToken = default)
        => _history.GetHistoryAsync(cancellationToken);

    public Task<City> AddInHistoryAsync(City city, CancellationToken cancellationToken = default)
        => _history.AddInHistoryAsync(city, cancellationToken);

    public Task<bool> RemoveFromHistoryAsync(int cityId, CancellationToken cancellationToken = default)
        => _history.RemoveFromHistoryAsync(cityId, cancellationToken);

    public Task<bool> IsRecentAsync(string cityName, CancellationToken cancellationToken = default)
        => _history.IsRecentAsync(cityName, cancellationToken);

    public Task ClearHistoryAsync(CancellationToken cancellationToken = default)
        => _history.ClearHistoryAsync(cancellationToken);
}