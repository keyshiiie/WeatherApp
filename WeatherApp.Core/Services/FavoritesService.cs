using Microsoft.Extensions.Logging;
using WeatherApp.Core.Models;
using WeatherApp.Core.Repositories;

namespace WeatherApp.Core.Services;

/// Реализация сервиса для работы с избранными городами
public class FavoritesService : IFavoritesService
{
    private readonly IWeatherRepository _repository;
    private readonly ILogger<FavoritesService> _logger;

    public FavoritesService(IWeatherRepository repository, ILogger<FavoritesService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<City>> GetFavoritesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _repository.GetAllCitiesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting favorites");
            return new List<City>();
        }
    }

    public async Task<City> AddFavoriteAsync(City city, CancellationToken cancellationToken = default)
    {
        try
        {
            if (city == null)
                throw new ArgumentNullException(nameof(city));

            if (string.IsNullOrWhiteSpace(city.Name))
                throw new ArgumentException("City name cannot be empty", nameof(city));

            // Проверяем, есть ли уже такой город
            var existing = await _repository.GetCityByNameAsync(city.Name, cancellationToken);
            if (existing != null)
            {
                _logger.LogInformation($"City {city.Name} already exists in favorites");
                return existing;
            }

            // Добавляем город
            var result = await _repository.AddCityAsync(city, cancellationToken);

            // Если это первый город, делаем его выбранным
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
            // Получаем город перед удалением
            var city = await _repository.GetCityByIdAsync(cityId, cancellationToken);
            if (city == null)
                return false;

            var result = await _repository.RemoveCityAsync(cityId, cancellationToken);

            if (result)
            {
                _logger.LogInformation($"City {city.Name} removed from favorites");

                // Если удалили последний выбранный город, выбираем другой
                if (city.IsLastSelected)
                {
                    await SelectFirstAvailableCityAsync(cancellationToken);
                }
            }

            return result;
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

            // Получаем город перед удалением
            var city = await _repository.GetCityByNameAsync(cityName, cancellationToken);
            if (city == null)
                return false;

            var result = await _repository.RemoveCityByNameAsync(cityName, cancellationToken);

            if (result)
            {
                _logger.LogInformation($"City {cityName} removed from favorites");

                // Если удалили последний выбранный город, выбираем другой
                if (city.IsLastSelected)
                {
                    await SelectFirstAvailableCityAsync(cancellationToken);
                }
            }

            return result;
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

            return await _repository.CityExistsAsync(cityName, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error checking if city is favorite: {cityName}");
            return false;
        }
    }

    public async Task<int> GetFavoritesCountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var cities = await _repository.GetAllCitiesAsync(cancellationToken);
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