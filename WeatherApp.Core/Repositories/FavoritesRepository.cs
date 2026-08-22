using Microsoft.Extensions.Logging;
using WeatherApp.Core.Models;

namespace WeatherApp.Core.Repositories;

public class FavoritesRepository : IFavoritesRepository
{
    private readonly IWeatherRepository _repository;
    private readonly ILogger<FavoritesRepository> _logger;

    public FavoritesRepository(
        IWeatherRepository repository,
        ILogger<FavoritesRepository> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<City> AddFavoriteAsync(City city, CancellationToken cancellationToken = default)
    {
        try
        {
            if (city == null)
                throw new ArgumentNullException(nameof(city));

            if (string.IsNullOrWhiteSpace(city.Name))
                throw new ArgumentException("City name cannot be empty", nameof(city));

            City? existing = null;
            if (city.Id > 0)
            {
                existing = await _repository.GetCityByIdAsync(city.Id, cancellationToken);
            }
            else
            {
                var allCities = await _repository.GetAllCitiesAsync(cancellationToken);
                existing = allCities.FirstOrDefault(c =>
                    Math.Abs(c.Latitude - city.Latitude) < 0.001 &&
                    Math.Abs(c.Longitude - city.Longitude) < 0.001);
            }

            if (existing != null)
            {
                _logger.LogInformation($"City {city.Name} already exists. Updating to favorite...");
                existing.IsFavorite = true;
                existing.AddedAt = DateTime.UtcNow;
                return await _repository.UpdateCityAsync(existing, cancellationToken);
            }

            city.IsFavorite = true;
            city.AddedAt = DateTime.UtcNow;
            var result = await _repository.AddCityAsync(city, cancellationToken);

            _logger.LogInformation($"City {city.Name} added to favorites");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error adding favorite city: {city?.Name}");
            throw;
        }
    }

    public async Task<bool> RemoveFavoriteAsync(int cityId, CancellationToken cancellationToken = default)
    {
        try
        {
            var city = await _repository.GetCityByIdAsync(cityId, cancellationToken);
            if (city == null)
                return false;

            city.IsFavorite = false;
            await _repository.UpdateCityAsync(city, cancellationToken);
            _logger.LogInformation($"City {city.Name} removed from favorites");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error removing favorite city with id: {cityId}");
            throw;
        }
    }

    public async Task<bool> IsFavoriteAsync(string cityName, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(cityName))
                return false;

            // ✅ Используем поиск по имени (как раньше)
            var allCities = await _repository.GetAllCitiesAsync(cancellationToken);
            var city = allCities.FirstOrDefault(c =>
                string.Equals(c.Name, cityName, StringComparison.OrdinalIgnoreCase));

            return city != null && city.IsFavorite;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error checking if city is favorite: {cityName}");
            return false;
        }
    }

    public async Task<List<City>> GetFavoritesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _repository.GetFavoriteCitiesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting favorites");
            return new List<City>();
        }
    }

    public async Task ClearAllFavoritesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var favorites = await _repository.GetFavoriteCitiesAsync(cancellationToken);
            foreach (var city in favorites)
            {
                city.IsFavorite = false;
                await _repository.UpdateCityAsync(city, cancellationToken);
            }
            _logger.LogInformation("All favorites cleared");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing all favorites");
            throw;
        }
    }
}