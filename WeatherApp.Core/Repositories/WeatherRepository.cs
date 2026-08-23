using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WeatherApp.Core.Data;
using WeatherApp.Core.Mappers;
using WeatherApp.Core.Models;
using WeatherApp.Core.Results;

namespace WeatherApp.Core.Repositories;

public class WeatherRepository : IWeatherRepository
{
    private readonly AppDbContext _context;
    private readonly ICityMapper _cityMapper;
    private readonly ILogger<WeatherRepository> _logger;

    public WeatherRepository(
        AppDbContext context,
        ICityMapper cityMapper,
        ILogger<WeatherRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _cityMapper = cityMapper ?? throw new ArgumentNullException(nameof(cityMapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<City>>> GetAllCitiesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _context.Cities
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);

            var cities = entities
                .Select(_cityMapper.MapToModel)
                .Where(c => c != null)
                .Select(c => c!)
                .ToList();

            return Result.Success(cities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all cities");
            return Result.Failure<List<City>>(new DatabaseError("Failed to retrieve cities", ex));
        }
    }

    public async Task<Result<City>> GetCityByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (id <= 0)
                return Result.Failure<City>(new ValidationError("Invalid city ID"));

            var entity = await _context.Cities
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (entity == null)
                return Result.Failure<City>(new NotFoundError("City", id.ToString()));

            var city = _cityMapper.MapToModel(entity);
            if (city == null)
                return Result.Failure<City>(new UnknownError("Failed to map city entity to model"));

            return Result.Success(city);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting city by id: {id}");
            return Result.Failure<City>(new DatabaseError($"Failed to retrieve city with ID {id}", ex));
        }
    }

    public async Task<Result<City>> AddCityAsync(City city, CancellationToken cancellationToken = default)
    {
        try
        {
            if (city == null)
                return Result.Failure<City>(new ValidationError("City cannot be null"));

            if (string.IsNullOrWhiteSpace(city.Name))
                return Result.Failure<City>(new ValidationError("City name is required"));

            if (string.IsNullOrWhiteSpace(city.Country))
                return Result.Failure<City>(new ValidationError("Country is required"));

            var existing = await _context.Cities
                .FirstOrDefaultAsync(c =>
                    c.Name == city.Name &&
                    c.Country == city.Country,
                    cancellationToken);

            if (existing != null)
            {
                _logger.LogInformation($"City '{city.Name}, {city.Country}' already exists. Updating...");

                if (Math.Abs(existing.Latitude - city.Latitude) > 0.001 ||
                    Math.Abs(existing.Longitude - city.Longitude) > 0.001)
                {
                    existing.Latitude = city.Latitude;
                    existing.Longitude = city.Longitude;
                    _logger.LogDebug($"Updated coordinates for {city.Name}: {city.Latitude}, {city.Longitude}");
                }

                if (!string.IsNullOrEmpty(city.Region) && existing.Region != city.Region)
                {
                    existing.Region = city.Region;
                }

                await _context.SaveChangesAsync(cancellationToken);

                var result = _cityMapper.MapToModel(existing);
                return Result.Success(result!);
            }

            // Создаем новый город
            var entity = _cityMapper.MapToEntity(city);
            if (entity == null)
                return Result.Failure<City>(new UnknownError("Failed to map city to entity"));

            await _context.Cities.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var newResult = _cityMapper.MapToModel(entity);
            if (newResult == null)
                return Result.Failure<City>(new UnknownError("Failed to map entity back to model"));

            _logger.LogInformation($"City '{city.Name}, {city.Country}' added successfully");
            return Result.Success(newResult);
        }
        catch (DbUpdateException ex) when (ex.InnerException is Microsoft.Data.Sqlite.SqliteException sqliteEx &&
                                          sqliteEx.SqliteErrorCode == 19)
        {
            _logger.LogWarning(ex, $"Duplicate entry for city: {city?.Name}, {city?.Country}");

            var existing = await _context.Cities
                .FirstOrDefaultAsync(c =>
                    c.Name == city.Name &&
                    c.Country == city.Country,
                    cancellationToken);

            if (existing != null)
            {
                var result = _cityMapper.MapToModel(existing);
                return Result.Success(result!);
            }

            return Result.Failure<City>(new DatabaseError($"City '{city?.Name}' already exists but cannot be retrieved", ex));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, $"Database error adding city: {city?.Name}");
            return Result.Failure<City>(new DatabaseError($"Failed to add city: {city?.Name}", ex));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error adding city: {city?.Name}");
            return Result.Failure<City>(new UnknownError($"Failed to add city: {city?.Name}", ex));
        }
    }

    public async Task<Result<City>> UpdateCityAsync(City city, CancellationToken cancellationToken = default)
    {
        try
        {
            if (city == null)
                return Result.Failure<City>(new ValidationError("City cannot be null"));

            if (city.Id <= 0)
                return Result.Failure<City>(new ValidationError("Invalid city ID"));

            var entity = await _context.Cities
                .FirstOrDefaultAsync(c => c.Id == city.Id, cancellationToken);

            if (entity == null)
                return Result.Failure<City>(new NotFoundError("City", city.Id.ToString()));

            entity.IsFavorite = city.IsFavorite;
            entity.IsRecent = city.IsRecent;
            entity.LastSearchedAt = city.LastSearchedAt;
            entity.IsLastSelected = city.IsLastSelected;

            await _context.SaveChangesAsync(cancellationToken);

            var result = _cityMapper.MapToModel(entity);
            if (result == null)
                return Result.Failure<City>(new UnknownError("Failed to map entity to model"));

            return Result.Success(result);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, $"Database error updating city: {city?.Name}");
            return Result.Failure<City>(new DatabaseError($"Failed to update city: {city?.Name}", ex));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating city: {city?.Name}");
            return Result.Failure<City>(new UnknownError($"Failed to update city: {city?.Name}", ex));
        }
    }

    public async Task<Result<bool>> RemoveCityAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (id <= 0)
                return Result.Failure<bool>(new ValidationError("Invalid city ID"));

            var entity = await _context.Cities
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (entity == null)
                return Result.Failure<bool>(new NotFoundError("City", id.ToString()));

            _context.Cities.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(true);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, $"Database error removing city with id: {id}");
            return Result.Failure<bool>(new DatabaseError($"Failed to remove city with ID {id}", ex));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error removing city with id: {id}");
            return Result.Failure<bool>(new UnknownError($"Failed to remove city with ID {id}", ex));
        }
    }

    public async Task<Result> SetLastSelectedCityAsync(int cityId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (cityId <= 0)
                return Result.Failure(new ValidationError("Invalid city ID"));

            var cityExists = await _context.Cities.AnyAsync(c => c.Id == cityId, cancellationToken);
            if (!cityExists)
                return Result.Failure(new NotFoundError("City", cityId.ToString()));

            await _context.Cities
                .Where(c => c.IsLastSelected)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(c => c.IsLastSelected, false),
                    cancellationToken);

            await _context.Cities
                .Where(c => c.Id == cityId)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(c => c.IsLastSelected, true),
                    cancellationToken);

            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, $"Database error setting last selected city: {cityId}");
            return Result.Failure(new DatabaseError($"Failed to set last selected city with ID {cityId}", ex));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error setting last selected city: {cityId}");
            return Result.Failure(new UnknownError($"Failed to set last selected city with ID {cityId}", ex));
        }
    }

    public async Task<Result<List<City>>> GetFavoriteCitiesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _context.Cities
                .Where(c => c.IsFavorite)
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);

            var cities = entities
                .Select(_cityMapper.MapToModel)
                .Where(c => c != null)
                .Select(c => c!)
                .ToList();

            return Result.Success(cities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting favorite cities");
            return Result.Failure<List<City>>(new DatabaseError("Failed to retrieve favorite cities", ex));
        }
    }

    public async Task<Result<List<City>>> GetRecentCitiesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _context.Cities
                .Where(c => c.IsRecent)
                .OrderByDescending(c => c.LastSearchedAt)
                .Take(20)
                .ToListAsync(cancellationToken);

            var cities = entities
                .Select(_cityMapper.MapToModel)
                .Where(c => c != null)
                .Select(c => c!)
                .ToList();

            return Result.Success(cities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent cities");
            return Result.Failure<List<City>>(new DatabaseError("Failed to retrieve recent cities", ex));
        }
    }

    public async Task<Result<bool>> IsCityFavoriteByNameAsync(string cityName, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(cityName))
                return Result.Failure<bool>(new ValidationError("City name cannot be empty"));

            var isFavorite = await _context.Cities
                .Where(c => c.Name == cityName)
                .Select(c => c.IsFavorite)
                .FirstOrDefaultAsync(cancellationToken);

            return Result.Success(isFavorite);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error checking if city is favorite: {cityName}");
            return Result.Failure<bool>(new DatabaseError($"Failed to check if city '{cityName}' is favorite", ex));
        }
    }

    public async Task<Result<City>> FindOrCreateCityAsync(
        string name,
        string country,
        double latitude,
        double longitude,
        string? region = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result.Failure<City>(new ValidationError("City name is required"));

            if (string.IsNullOrWhiteSpace(country))
                return Result.Failure<City>(new ValidationError("Country is required"));

            var existing = await _context.Cities
                .FirstOrDefaultAsync(c =>
                    c.Name == name &&
                    c.Country == country,
                    cancellationToken);

            if (existing != null)
            {
                if (Math.Abs(existing.Latitude - latitude) > 0.001 ||
                    Math.Abs(existing.Longitude - longitude) > 0.001)
                {
                    existing.Latitude = latitude;
                    existing.Longitude = longitude;
                    await _context.SaveChangesAsync(cancellationToken);
                }

                var city = _cityMapper.MapToModel(existing);
                return Result.Success(city!);
            }

            var newCity = new City
            {
                Name = name,
                Country = country,
                Region = region ?? country,
                Latitude = latitude,
                Longitude = longitude,
                AddedAt = DateTime.UtcNow,
                IsLastSelected = false,
                IsFavorite = false,
                IsRecent = false,
                LastSearchedAt = DateTime.UtcNow
            };

            var entity = _cityMapper.MapToEntity(newCity);
            if (entity == null)
                return Result.Failure<City>(new UnknownError("Failed to map city to entity"));

            await _context.Cities.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var result = _cityMapper.MapToModel(entity);
            if (result == null)
                return Result.Failure<City>(new UnknownError("Failed to map entity to model"));

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error finding or creating city: {name}, {country}");
            return Result.Failure<City>(new UnknownError($"Failed to find or create city: {name}", ex));
        }
    }
}