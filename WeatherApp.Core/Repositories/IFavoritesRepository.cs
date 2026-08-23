using WeatherApp.Core.Models;
using WeatherApp.Core.Results;

namespace WeatherApp.Core.Repositories
{
    public interface IFavoritesRepository
    {
        Task<Result<List<City>>> GetFavoritesAsync(CancellationToken cancellationToken = default);
        Task<Result<City>> AddFavoriteAsync(City city, CancellationToken cancellationToken = default);
        Task<Result<bool>> RemoveFavoriteAsync(int cityId, CancellationToken cancellationToken = default);
        Task<Result<bool>> IsFavoriteAsync(string cityName, CancellationToken cancellationToken = default);
        Task<Result> ClearAllFavoritesAsync(CancellationToken cancellationToken = default);
    }
}
