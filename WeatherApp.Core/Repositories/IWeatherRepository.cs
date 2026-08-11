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
        Task<City?> GetCityByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<City?> GetCityByCoordinatesAsync(double latitude, double longitude, CancellationToken cancellationToken = default);
        Task<City?> GetLastSelectedCityAsync(CancellationToken cancellationToken = default);
        Task<City> AddCityAsync(City city, CancellationToken cancellationToken = default);
        Task<City> UpdateCityAsync(City city, CancellationToken cancellationToken = default);
        Task<bool> RemoveCityAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> RemoveCityByNameAsync(string name, CancellationToken cancellationToken = default);
        Task SetLastSelectedCityAsync(int cityId, CancellationToken cancellationToken = default);
        Task<bool> CityExistsAsync(string name, CancellationToken cancellationToken = default);
        Task ClearWeatherCacheAsync(int cityId, CancellationToken cancellationToken = default);
        Task SaveWeatherCacheAsync(int cityId, WeatherData weatherData, CancellationToken cancellationToken = default);
        Task<WeatherData?> GetWeatherCacheAsync(int cityId, CancellationToken cancellationToken = default);
    }
}
