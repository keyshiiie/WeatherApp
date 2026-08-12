using System;
using System.Collections.Generic;
using System.Text;
using WeatherApp.Core.Models;

namespace WeatherApp.Core.Services
{
    public interface IFavoritesService
    {
        Task<List<City>> GetFavoritesAsync(CancellationToken cancellationToken = default);
        Task<City> AddFavoriteAsync(City city, CancellationToken cancellationToken = default);
        Task<bool> RemoveFavoriteAsync(int cityId, CancellationToken cancellationToken = default);
        Task<bool> RemoveFavoriteByNameAsync(string cityName, CancellationToken cancellationToken = default);
        Task<City?> GetLastFavoriteAsync(CancellationToken cancellationToken = default);
        Task SetLastFavoriteAsync(City city, CancellationToken cancellationToken = default);
        Task<bool> IsFavoriteAsync(string cityName, CancellationToken cancellationToken = default);
        Task<int> GetFavoritesCountAsync(CancellationToken cancellationToken = default);
        Task ClearAllFavoritesAsync(CancellationToken cancellationToken = default);
    }
}
