// WeatherApp.Core/Repositories/HistoryRepository.cs
using Microsoft.Extensions.Logging;
using WeatherApp.Core.Models;
using WeatherApp.Core.Results;

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

    public async Task<Result<List<City>>> GetHistoryAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _repository.GetRecentCitiesAsync(cancellationToken);
            if (result.IsFailure)
                return Result.Failure<List<City>>(result.Error!);

            return Result.Success(result.Value ?? new List<City>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting history");
            return Result.Failure<List<City>>(new UnknownError("Failed to retrieve history", ex));
        }
    }

    public async Task<Result<City>> AddInHistoryAsync(City city, CancellationToken cancellationToken = default)
    {
        try
        {
            if (city == null)
                return Result.Failure<City>(new ValidationError("City cannot be null"));

            if (string.IsNullOrWhiteSpace(city.Name))
                return Result.Failure<City>(new ValidationError("City name cannot be empty"));

            if (string.IsNullOrWhiteSpace(city.Country))
                return Result.Failure<City>(new ValidationError("Country is required"));

            // ✅ Получаем все города для поиска
            var allCitiesResult = await _repository.GetAllCitiesAsync(cancellationToken);
            if (allCitiesResult.IsFailure)
                return Result.Failure<City>(allCitiesResult.Error!);

            var allCities = allCitiesResult.Value ?? new List<City>();

            // ✅ Ищем по Name + Country (основной поиск)
            City? existing = allCities.FirstOrDefault(c =>
                string.Equals(c.Name, city.Name, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(c.Country, city.Country, StringComparison.OrdinalIgnoreCase));

            // ✅ Если не нашли по Name+Country, ищем по координатам (для обратной совместимости)
            if (existing == null)
            {
                existing = allCities.FirstOrDefault(c =>
                    Math.Abs(c.Latitude - city.Latitude) < 0.001 &&
                    Math.Abs(c.Longitude - city.Longitude) < 0.001);
            }

            if (existing != null)
            {
                _logger.LogInformation($"City '{city.Name}, {city.Country}' already exists. Updating in history...");

                existing.IsRecent = true;
                existing.LastSearchedAt = DateTime.UtcNow;

                // Обновляем координаты, если они изменились
                if (Math.Abs(existing.Latitude - city.Latitude) > 0.001 ||
                    Math.Abs(existing.Longitude - city.Longitude) > 0.001)
                {
                    existing.Latitude = city.Latitude;
                    existing.Longitude = city.Longitude;
                    _logger.LogDebug($"Updated coordinates for {existing.Name}");
                }

                // Обновляем регион, если он изменился
                if (!string.IsNullOrWhiteSpace(city.Region) && existing.Region != city.Region)
                {
                    existing.Region = city.Region;
                }

                var updateResult = await _repository.UpdateCityAsync(existing, cancellationToken);
                if (updateResult.IsFailure)
                    return Result.Failure<City>(updateResult.Error!);

                _logger.LogInformation($"City '{city.Name}' updated in history");
                return Result.Success(updateResult.Value!);
            }

            // ✅ Создаем новый город
            _logger.LogInformation($"Adding new city to history: {city.Name}, {city.Country}");

            city.IsRecent = true;
            city.LastSearchedAt = DateTime.UtcNow;
            city.IsFavorite = false;
            city.IsLastSelected = false;
            city.AddedAt = DateTime.UtcNow;

            var addResult = await _repository.AddCityAsync(city, cancellationToken);
            if (addResult.IsFailure)
                return Result.Failure<City>(addResult.Error!);

            _logger.LogInformation($"City '{city.Name}' added to history");
            return Result.Success(addResult.Value!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error adding city to history: {city?.Name}");
            return Result.Failure<City>(new UnknownError($"Failed to add city to history: {city?.Name}", ex));
        }
    }

    public async Task<Result<bool>> RemoveFromHistoryAsync(int cityId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (cityId <= 0)
                return Result.Failure<bool>(new ValidationError("Invalid city ID"));

            var cityResult = await _repository.GetCityByIdAsync(cityId, cancellationToken);
            if (cityResult.IsFailure)
                return Result.Failure<bool>(cityResult.Error!);

            var city = cityResult.Value!;

            // ✅ Проверяем, не является ли город избранным
            // Если город в избранном, оставляем его, но убираем флаг Recent
            if (city.IsFavorite)
            {
                city.IsRecent = false;
                var updateResult = await _repository.UpdateCityAsync(city, cancellationToken);
                if (updateResult.IsFailure)
                    return Result.Failure<bool>(updateResult.Error!);

                _logger.LogInformation($"City '{city.Name}' removed from history (but remains in favorites)");
                return Result.Success(true);
            }

            // Если город не в избранном, удаляем его полностью
            var removeResult = await _repository.RemoveCityAsync(cityId, cancellationToken);
            if (removeResult.IsFailure)
                return Result.Failure<bool>(removeResult.Error!);

            _logger.LogInformation($"City '{city.Name}' removed from history");
            return Result.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error removing city from history: {cityId}");
            return Result.Failure<bool>(new UnknownError($"Failed to remove city from history with ID {cityId}", ex));
        }
    }

    public async Task<Result<bool>> IsRecentAsync(string cityName, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(cityName))
                return Result.Failure<bool>(new ValidationError("City name cannot be empty"));

            var allCitiesResult = await _repository.GetAllCitiesAsync(cancellationToken);
            if (allCitiesResult.IsFailure)
                return Result.Failure<bool>(allCitiesResult.Error!);

            var city = allCitiesResult.Value?.FirstOrDefault(c =>
                string.Equals(c.Name, cityName, StringComparison.OrdinalIgnoreCase));

            return Result.Success(city != null && city.IsRecent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error checking if city is recent: {cityName}");
            return Result.Failure<bool>(new UnknownError($"Failed to check if city '{cityName}' is recent", ex));
        }
    }

    public async Task<Result> ClearHistoryAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var historyResult = await _repository.GetRecentCitiesAsync(cancellationToken);
            if (historyResult.IsFailure)
                return Result.Failure(historyResult.Error!);

            var history = historyResult.Value ?? new List<City>();

            foreach (var city in history)
            {
                // ✅ Если город в избранном, только убираем флаг Recent
                if (city.IsFavorite)
                {
                    city.IsRecent = false;
                    var updateResult = await _repository.UpdateCityAsync(city, cancellationToken);
                    if (updateResult.IsFailure)
                        _logger.LogWarning($"Failed to update city {city.Name}: {updateResult.Error}");
                }
                else
                {
                    // Если не в избранном, удаляем
                    var removeResult = await _repository.RemoveCityAsync(city.Id, cancellationToken);
                    if (removeResult.IsFailure)
                        _logger.LogWarning($"Failed to remove city {city.Name}: {removeResult.Error}");
                }
            }

            _logger.LogInformation($"History cleared ({history.Count} cities)");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing history");
            return Result.Failure(new UnknownError("Failed to clear history", ex));
        }
    }
}