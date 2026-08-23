// WeatherApp.Core/Repositories/FavoritesRepository.cs
using Microsoft.Extensions.Logging;
using WeatherApp.Core.Models;
using WeatherApp.Core.Results;

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

    public async Task<Result<City>> AddFavoriteAsync(City city, CancellationToken cancellationToken = default)
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
                _logger.LogInformation($"City '{city.Name}, {city.Country}' already exists. Updating to favorite...");

                existing.IsFavorite = true;
                existing.AddedAt = DateTime.UtcNow;

                // ✅ Если город был в истории, оставляем его там
                if (!existing.IsRecent)
                {
                    existing.IsRecent = true;
                    existing.LastSearchedAt = DateTime.UtcNow;
                }

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

                _logger.LogInformation($"City '{city.Name}' added to favorites");
                return Result.Success(updateResult.Value!);
            }

            // ✅ Создаем новый город
            _logger.LogInformation($"Adding new favorite city: {city.Name}, {city.Country}");

            city.IsFavorite = true;
            city.AddedAt = DateTime.UtcNow;
            city.IsRecent = true; // ✅ Добавляем в историю автоматически
            city.IsLastSelected = false;
            city.LastSearchedAt = DateTime.UtcNow;

            var addResult = await _repository.AddCityAsync(city, cancellationToken);
            if (addResult.IsFailure)
                return Result.Failure<City>(addResult.Error!);

            _logger.LogInformation($"City '{city.Name}' added to favorites");
            return Result.Success(addResult.Value!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error adding favorite city: {city?.Name}");
            return Result.Failure<City>(new UnknownError($"Failed to add favorite city: {city?.Name}", ex));
        }
    }

    public async Task<Result<bool>> RemoveFavoriteAsync(int cityId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (cityId <= 0)
                return Result.Failure<bool>(new ValidationError("Invalid city ID"));

            var cityResult = await _repository.GetCityByIdAsync(cityId, cancellationToken);
            if (cityResult.IsFailure)
                return Result.Failure<bool>(cityResult.Error!);

            var city = cityResult.Value!;

            // ✅ Если город в истории, просто убираем флаг Favorite
            if (city.IsRecent)
            {
                city.IsFavorite = false;
                var updateResult = await _repository.UpdateCityAsync(city, cancellationToken);
                if (updateResult.IsFailure)
                    return Result.Failure<bool>(updateResult.Error!);

                _logger.LogInformation($"City '{city.Name}' removed from favorites (remains in history)");
                return Result.Success(true);
            }

            // Если город не в истории, удаляем полностью
            var removeResult = await _repository.RemoveCityAsync(cityId, cancellationToken);
            if (removeResult.IsFailure)
                return Result.Failure<bool>(removeResult.Error!);

            _logger.LogInformation($"City '{city.Name}' removed from favorites");
            return Result.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error removing favorite city with id: {cityId}");
            return Result.Failure<bool>(new UnknownError($"Failed to remove favorite city with ID {cityId}", ex));
        }
    }

    public async Task<Result<bool>> IsFavoriteAsync(string cityName, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(cityName))
                return Result.Failure<bool>(new ValidationError("City name cannot be empty"));

            var result = await _repository.IsCityFavoriteByNameAsync(cityName, cancellationToken);
            if (result.IsFailure)
                return Result.Failure<bool>(result.Error!);

            return Result.Success(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error checking if city is favorite: {cityName}");
            return Result.Failure<bool>(new UnknownError($"Failed to check if city '{cityName}' is favorite", ex));
        }
    }

    public async Task<Result<List<City>>> GetFavoritesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _repository.GetFavoriteCitiesAsync(cancellationToken);
            if (result.IsFailure)
                return Result.Failure<List<City>>(result.Error!);

            return Result.Success(result.Value ?? new List<City>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting favorites");
            return Result.Failure<List<City>>(new UnknownError("Failed to retrieve favorites", ex));
        }
    }

    public async Task<Result> ClearAllFavoritesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var favoritesResult = await _repository.GetFavoriteCitiesAsync(cancellationToken);
            if (favoritesResult.IsFailure)
                return Result.Failure(favoritesResult.Error!);

            var favorites = favoritesResult.Value ?? new List<City>();

            foreach (var city in favorites)
            {
                // ✅ Если город в истории, только убираем флаг Favorite
                if (city.IsRecent)
                {
                    city.IsFavorite = false;
                    var updateResult = await _repository.UpdateCityAsync(city, cancellationToken);
                    if (updateResult.IsFailure)
                        _logger.LogWarning($"Failed to update city {city.Name}: {updateResult.Error}");
                }
                else
                {
                    // Если не в истории, удаляем
                    var removeResult = await _repository.RemoveCityAsync(city.Id, cancellationToken);
                    if (removeResult.IsFailure)
                        _logger.LogWarning($"Failed to remove city {city.Name}: {removeResult.Error}");
                }
            }

            _logger.LogInformation($"All favorites cleared ({favorites.Count} cities)");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing all favorites");
            return Result.Failure(new UnknownError("Failed to clear all favorites", ex));
        }
    }
}