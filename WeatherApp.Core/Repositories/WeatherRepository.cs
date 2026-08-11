using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WeatherApp.Core.Data;
using WeatherApp.Core.Mappers;
using WeatherApp.Core.Models;

namespace WeatherApp.Core.Repositories;
public class WeatherRepository : IWeatherRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<WeatherRepository> _logger;

    public WeatherRepository(AppDbContext context, ILogger<WeatherRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<City>> GetAllCitiesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _context.Cities
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);

            return entities.Select(CityMapper.ToModel)
                .Where(c => c != null)
                .Select(c => c!)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all cities");
            return new List<City>();
        }
    }

    public async Task<City?> GetCityByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _context.Cities
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            return CityMapper.ToModel(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting city by id: {id}");
            return null;
        }
    }

    public async Task<City?> GetCityByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            var entity = await _context.Cities
                .FirstOrDefaultAsync(c => c.Name == name, cancellationToken);

            return CityMapper.ToModel(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting city by name: {name}");
            return null;
        }
    }

    public async Task<City?> GetCityByCoordinatesAsync(double latitude, double longitude, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _context.Cities
                .FirstOrDefaultAsync(c => c.Latitude == latitude && c.Longitude == longitude, cancellationToken);

            return CityMapper.ToModel(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting city by coordinates: {latitude}, {longitude}");
            return null;
        }
    }

    public async Task<City?> GetLastSelectedCityAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _context.Cities
                .FirstOrDefaultAsync(c => c.IsLastSelected, cancellationToken);

            return CityMapper.ToModel(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting last selected city");
            return null;
        }
    }

    public async Task<City> AddCityAsync(City city, CancellationToken cancellationToken = default)
    {
        try
        {
            if (city == null)
                throw new ArgumentNullException(nameof(city));

            // Проверяем, существует ли уже такой город
            var existing = await GetCityByNameAsync(city.Name!, cancellationToken);
            if (existing != null)
                return existing;

            var entity = CityMapper.ToEntity(city);
            if (entity == null)
                throw new InvalidOperationException("Failed to map city to entity");

            await _context.Cities.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return CityMapper.ToModel(entity) ?? throw new InvalidOperationException("Failed to map entity back to model");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error adding city: {city?.Name}");
            throw;
        }
    }

    public async Task<City> UpdateCityAsync(City city, CancellationToken cancellationToken = default)
    {
        try
        {
            if (city == null)
                throw new ArgumentNullException(nameof(city));

            var entity = await _context.Cities
                .FirstOrDefaultAsync(c => c.Id == city.Id, cancellationToken);

            if (entity == null)
                throw new InvalidOperationException($"City with id {city.Id} not found");

            // Обновляем поля
            entity.Name = city.Name;
            entity.Country = city.Country;
            entity.Latitude = city.Latitude;
            entity.Longitude = city.Longitude;
            entity.IsLastSelected = city.IsLastSelected;

            _context.Cities.Update(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return CityMapper.ToModel(entity) ?? throw new InvalidOperationException("Failed to map entity to model");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating city: {city?.Name}");
            throw;
        }
    }

    public async Task<bool> RemoveCityAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _context.Cities
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (entity == null)
                return false;

            _context.Cities.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error removing city with id: {id}");
            return false;
        }
    }

    public async Task<bool> RemoveCityByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            var entity = await _context.Cities
                .FirstOrDefaultAsync(c => c.Name == name, cancellationToken);

            if (entity == null)
                return false;

            _context.Cities.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error removing city by name: {name}");
            return false;
        }
    }

    public async Task SetLastSelectedCityAsync(int cityId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Сбрасываем флаг у всех городов
            await _context.Cities
                .Where(c => c.IsLastSelected)
                .ForEachAsync(c => c.IsLastSelected = false, cancellationToken);

            // Устанавливаем флаг для выбранного города
            var city = await _context.Cities
                .FirstOrDefaultAsync(c => c.Id == cityId, cancellationToken);

            if (city != null)
            {
                city.IsLastSelected = true;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error setting last selected city: {cityId}");
            throw;
        }
    }

    public async Task<bool> CityExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            return await _context.Cities
                .AnyAsync(c => c.Name == name, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error checking city existence: {name}");
            return false;
        }
    }

    public async Task ClearWeatherCacheAsync(int cityId, CancellationToken cancellationToken = default)
    {
        try
        {
            var cache = await _context.WeatherCache
                .FirstOrDefaultAsync(w => w.CityId == cityId, cancellationToken);

            if (cache != null)
            {
                _context.WeatherCache.Remove(cache);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error clearing weather cache for city: {cityId}");
            throw;
        }
    }

    public async Task SaveWeatherCacheAsync(int cityId, WeatherData weatherData, CancellationToken cancellationToken = default)
    {
        try
        {
            if (weatherData == null)
                throw new ArgumentNullException(nameof(weatherData));

            // Удаляем старый кэш, если есть
            await ClearWeatherCacheAsync(cityId, cancellationToken);

            // Создаем новый кэш
            var cacheEntity = WeatherCacheMapper.ToEntity(weatherData, cityId);
            if (cacheEntity == null)
                throw new InvalidOperationException("Failed to map weather data to cache entity");

            await _context.WeatherCache.AddAsync(cacheEntity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error saving weather cache for city: {cityId}");
            throw;
        }
    }

    public async Task<WeatherData?> GetWeatherCacheAsync(int cityId, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheEntity = await _context.WeatherCache
                .FirstOrDefaultAsync(w => w.CityId == cityId, cancellationToken);

            if (cacheEntity == null)
                return null;

            // Проверяем, не истек ли кэш
            if (!cacheEntity.IsValid)
            {
                _context.WeatherCache.Remove(cacheEntity);
                await _context.SaveChangesAsync(cancellationToken);
                return null;
            }

            return WeatherCacheMapper.ToModel(cacheEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting weather cache for city: {cityId}");
            return null;
        }
    }
}