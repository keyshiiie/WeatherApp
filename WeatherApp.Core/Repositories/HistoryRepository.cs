using Microsoft.Extensions.Logging;
using WeatherApp.Core.Models;

namespace WeatherApp.Core.Repositories;

public class HistoryRepository : IHistoryRepository
{
    private readonly IWeatherRepository _repository;
    private readonly ILogger<HistoryRepository> _logger;

    public HistoryRepository(
        IWeatherRepository repository,
        ILogger<HistoryRepository> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<City>> GetHistoryAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _repository.GetRecentCitiesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting history");
            return new List<City>();
        }
    }

    public async Task<City> AddInHistoryAsync(City city, CancellationToken cancellationToken = default)
    {
        try
        {
            if (city == null)
                throw new ArgumentNullException(nameof(city));

            // Проверяем существование по ID (если есть) или по координатам
            City? existing = null;
            if (city.Id > 0)
            {
                existing = await _repository.GetCityByIdAsync(city.Id, cancellationToken);
            }
            else
            {
                // Если ID нет, ищем по координатам среди всех городов
                var allCities = await _repository.GetAllCitiesAsync(cancellationToken);
                existing = allCities.FirstOrDefault(c =>
                    Math.Abs(c.Latitude - city.Latitude) < 0.001 &&
                    Math.Abs(c.Longitude - city.Longitude) < 0.001);
            }

            if (existing != null)
            {
                existing.IsRecent = true;
                existing.LastSearchedAt = DateTime.UtcNow;
                await _repository.UpdateCityAsync(existing, cancellationToken);
                return existing;
            }

            city.IsRecent = true;
            city.LastSearchedAt = DateTime.UtcNow;
            city.IsFavorite = false;
            var result = await _repository.AddCityAsync(city, cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error adding city to history: {city?.Name}");
            throw;
        }
    }

    public async Task<bool> RemoveFromHistoryAsync(int cityId, CancellationToken cancellationToken = default)
    {
        try
        {
            var city = await _repository.GetCityByIdAsync(cityId, cancellationToken);
            if (city == null) return false;

            city.IsRecent = false;
            await _repository.UpdateCityAsync(city, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error removing city from history: {cityId}");
            throw;
        }
    }

    public async Task<bool> IsRecentAsync(string cityName, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(cityName))
                return false;

            var allCities = await _repository.GetAllCitiesAsync(cancellationToken);
            var city = allCities.FirstOrDefault(c =>
                string.Equals(c.Name, cityName, StringComparison.OrdinalIgnoreCase));

            return city != null && city.IsRecent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error checking if city is recent: {cityName}");
            return false;
        }
    }

    public async Task ClearHistoryAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var list = await _repository.GetRecentCitiesAsync(cancellationToken);
            foreach (var city in list)
            {
                city.IsRecent = false;
                await _repository.UpdateCityAsync(city, cancellationToken);
            }
            _logger.LogInformation($"History cleared ({list.Count} cities)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing history");
            throw;
        }
    }
}