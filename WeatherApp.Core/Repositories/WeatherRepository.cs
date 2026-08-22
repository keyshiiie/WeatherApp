using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WeatherApp.Core.Data;
using WeatherApp.Core.Mappers;
using WeatherApp.Core.Models;

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

    public async Task<List<City>> GetAllCitiesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _context.Cities
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);

            return entities
                .Select(_cityMapper.MapToModel)
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

            return _cityMapper.MapToModel(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting city by id: {id}");
            return null;
        }
    }

    public async Task<City> AddCityAsync(City city, CancellationToken cancellationToken = default)
    {
        try
        {
            if (city == null)
                throw new ArgumentNullException(nameof(city));

            var existing = await GetCityByCoordinatesInternalAsync(
                city.Latitude,
                city.Longitude,
                cancellationToken);

            if (existing != null)
                return existing;

            var entity = _cityMapper.MapToEntity(city);
            if (entity == null)
                throw new InvalidOperationException("Failed to map city to entity");

            await _context.Cities.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var result = _cityMapper.MapToModel(entity);
            if (result == null)
                throw new InvalidOperationException("Failed to map entity back to model");

            return result;
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
                throw new KeyNotFoundException($"City with Id {city.Id} not found");

            entity.IsFavorite = city.IsFavorite;
            entity.IsRecent = city.IsRecent;
            entity.LastSearchedAt = city.LastSearchedAt;
            entity.IsLastSelected = city.IsLastSelected;

            await _context.SaveChangesAsync(cancellationToken);

            var result = _cityMapper.MapToModel(entity);
            return result ?? throw new InvalidOperationException("Failed to map entity to model");
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
            throw;
        }
    }

    public async Task SetLastSelectedCityAsync(int cityId, CancellationToken cancellationToken = default)
    {
        try
        {
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error setting last selected city: {cityId}");
            throw;
        }
    }

    public async Task<List<City>> GetFavoriteCitiesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _context.Cities
                .Where(c => c.IsFavorite)
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);

            return entities
                .Select(_cityMapper.MapToModel)
                .Where(c => c != null)
                .Select(c => c!)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting favorite cities");
            return new List<City>();
        }
    }

    public async Task<List<City>> GetRecentCitiesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _context.Cities
                .Where(c => c.IsRecent)
                .OrderByDescending(c => c.LastSearchedAt)
                .Take(20)
                .ToListAsync(cancellationToken);

            return entities
                .Select(_cityMapper.MapToModel)
                .Where(c => c != null)
                .Select(c => c!)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent cities");
            return new List<City>();
        }
    }

    private async Task<City?> GetCityByCoordinatesInternalAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken)
    {
        try
        {
            var entity = await _context.Cities
                .FirstOrDefaultAsync(c => c.Latitude == latitude && c.Longitude == longitude, cancellationToken);

            return _cityMapper.MapToModel(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting city by coordinates: {latitude}, {longitude}");
            return null;
        }
    }
}