using Microsoft.Extensions.Logging;
using WeatherApp.Core.Models;

namespace WeatherApp.Core.Repositories;

/// Реализация сервиса для работы с избранными городами
public class FavoritesRepository : IFavoritesRepository
{
    private readonly IWeatherRepository _repository;
    private readonly ILogger<FavoritesRepository> _logger;

    public FavoritesRepository(IWeatherRepository repository, ILogger<FavoritesRepository> logger)
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

            var existing = await _repository.GetCityByNameAsync(city.Name, cancellationToken);
            if (existing != null)
            {
                _logger.LogInformation($"City {city.Name} already exists. Updating to favorite...");

                existing.IsFavorite = true;
                existing.AddedAt = DateTime.UtcNow;
                await _repository.UpdateCityAsync(existing, cancellationToken);

                return existing;
            }

            var result = await _repository.AddCityAsync(city, cancellationToken);

            var count = await _repository.GetAllCitiesAsync(cancellationToken);
            if (count.Count == 1)
            {
                await _repository.SetLastSelectedCityAsync(result.Id, cancellationToken);
            }

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
            return false;
        }
    }

    public async Task<bool> RemoveFavoriteByNameAsync(string cityName, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(cityName))
                return false;

            var city = await _repository.GetCityByNameAsync(cityName, cancellationToken);
            if (city == null)
                return false;

            city.IsFavorite = false;
            await _repository.UpdateCityAsync(city, cancellationToken);
            _logger.LogInformation($"City {cityName} removed from favorites");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error removing favorite city by name: {cityName}");
            return false;
        }
    }

    public async Task<City?> GetLastFavoriteAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _repository.GetLastSelectedCityAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting last favorite");
            return null;
        }
    }

    public async Task SetLastFavoriteAsync(City city, CancellationToken cancellationToken = default)
    {
        try
        {
            if (city == null)
                throw new ArgumentNullException(nameof(city));

            if (city.Id <= 0)
                throw new ArgumentException("Invalid city id", nameof(city));

            // Проверяем, существует ли город
            var existing = await _repository.GetCityByIdAsync(city.Id, cancellationToken);
            if (existing == null)
                throw new InvalidOperationException($"City with id {city.Id} not found");

            await _repository.SetLastSelectedCityAsync(city.Id, cancellationToken);

            _logger.LogInformation($"City {city.Name} set as last favorite");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error setting last favorite: {city?.Name}");
            throw;
        }
    }

    public async Task<bool> IsFavoriteAsync(string cityName, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(cityName))
                return false;

            var city = await _repository.GetCityByNameAsync(cityName, cancellationToken);
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

    public async Task<int> GetFavoritesCountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var cities = await _repository.GetFavoriteCitiesAsync(cancellationToken);
            return cities.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting favorites count");
            return 0;
        }
    }

    public async Task ClearAllFavoritesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var cities = await _repository.GetAllCitiesAsync(cancellationToken);
            foreach (var city in cities)
            {
                await _repository.RemoveCityAsync(city.Id, cancellationToken);
            }
            _logger.LogInformation("All favorites cleared");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing all favorites");
            throw;
        }
    }

    private async Task SelectFirstAvailableCityAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var cities = await _repository.GetAllCitiesAsync(cancellationToken);
            if (cities.Count > 0)
            {
                var firstCity = cities[0];
                await _repository.SetLastSelectedCityAsync(firstCity.Id, cancellationToken);
                _logger.LogInformation($"Auto-selected city: {firstCity.Name}");
            }
            else
            {
                _logger.LogInformation("No cities left in favorites");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error selecting first available city");
        }
    }
}