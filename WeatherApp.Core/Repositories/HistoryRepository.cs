using Microsoft.Extensions.Logging;
using WeatherApp.Core.Models;

namespace WeatherApp.Core.Repositories;

public class HistoryRepository : IHistoryRepository
{
    private readonly IWeatherRepository _repository;
    private readonly ILogger<HistoryRepository> _logger;

    public HistoryRepository(IWeatherRepository repository, ILogger<HistoryRepository> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<City>> GetHistoryAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.GetRecentCitiesAsync(cancellationToken);
    }

    public async Task<City> AddInHistoryAsync(City city, CancellationToken cancellationToken = default)
    {
        try
        {
            if (city == null) throw new ArgumentNullException(nameof(city));

            var existing = await _repository.GetCityByNameAsync(city.Name, cancellationToken);
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
        var city = await _repository.GetCityByIdAsync(cityId, cancellationToken);
        if (city == null) return false;

        city.IsRecent = false;
        await _repository.UpdateCityAsync(city, cancellationToken);
        return true;
    }

    public async Task<bool> IsRecentAsync(string cityName, CancellationToken cancellationToken = default)
    {
        var city = await _repository.GetCityByNameAsync(cityName, cancellationToken);
        return city != null && city.IsRecent;
    }

    public async Task ClearHistoryAsync(CancellationToken cancellationToken = default)
    {
        var list = await _repository.GetRecentCitiesAsync(cancellationToken);
        foreach (var city in list)
        {
            city.IsRecent = false;
            await _repository.UpdateCityAsync(city, cancellationToken);
        }
    }
}