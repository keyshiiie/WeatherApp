using WeatherApp.Core.Models;
using WeatherApp.Core.Results;

namespace WeatherApp.Core.Repositories;

public interface IWeatherRepository
{
    Task<Result<List<City>>> GetAllCitiesAsync(CancellationToken cancellationToken = default);
    Task<Result<City>> GetCityByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<City>> AddCityAsync(City city, CancellationToken cancellationToken = default);
    Task<Result<City>> UpdateCityAsync(City city, CancellationToken cancellationToken = default);
    Task<Result<bool>> RemoveCityAsync(int id, CancellationToken cancellationToken = default);
    Task<Result> SetLastSelectedCityAsync(int cityId, CancellationToken cancellationToken = default);
    Task<Result<List<City>>> GetFavoriteCitiesAsync(CancellationToken cancellationToken = default);
    Task<Result<List<City>>> GetRecentCitiesAsync(CancellationToken cancellationToken = default);
    Task<Result<bool>> IsCityFavoriteByNameAsync(string cityName, CancellationToken cancellationToken = default);
    Task<Result<City>> FindOrCreateCityAsync(
        string name,
        string country,
        double latitude,
        double longitude,
        string? region = null,
        CancellationToken cancellationToken = default);
}