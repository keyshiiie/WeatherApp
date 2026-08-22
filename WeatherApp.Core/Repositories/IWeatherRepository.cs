using System;
using System.Collections.Generic;
using System.Text;
using WeatherApp.Core.Models;

namespace WeatherApp.Core.Repositories
{
    public interface IWeatherRepository
    {
        Task<List<City>> GetAllCitiesAsync(CancellationToken cancellationToken = default);
        Task<City?> GetCityByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<City> AddCityAsync(City city, CancellationToken cancellationToken = default);
        Task<City> UpdateCityAsync(City city, CancellationToken cancellationToken = default);
        Task<bool> RemoveCityAsync(int id, CancellationToken cancellationToken = default);
        Task SetLastSelectedCityAsync(int cityId, CancellationToken cancellationToken = default);
        Task<List<City>> GetFavoriteCitiesAsync(CancellationToken cancellationToken = default);
        Task<List<City>> GetRecentCitiesAsync(CancellationToken cancellationToken = default);
    }
}
