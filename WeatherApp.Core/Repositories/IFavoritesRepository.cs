using WeatherApp.Core.Models;

namespace WeatherApp.Core.Repositories
{
    public interface IFavoritesRepository
    {
        Task<List<City>> GetFavoritesAsync(CancellationToken cancellationToken = default);
        Task<City> AddFavoriteAsync(City city, CancellationToken cancellationToken = default);
        Task<bool> RemoveFavoriteAsync(int cityId, CancellationToken cancellationToken = default);
        Task<bool> IsFavoriteAsync(string cityName, CancellationToken cancellationToken = default);
        Task ClearAllFavoritesAsync(CancellationToken cancellationToken = default);
    }
}
