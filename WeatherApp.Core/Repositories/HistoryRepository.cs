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

            // Проверяем, есть ли уже такой город в БД (чтобы не было дубликатов)
            var existing = await _repository.GetCityByNameAsync(city.Name, cancellationToken);
            if (existing != null)
            {
                // Если есть, просто обновляем дату и делаем его "недавним"
                existing.IsRecent = true;
                existing.LastSearchedAt = DateTime.UtcNow;
                await _repository.UpdateCityAsync(existing, cancellationToken);
                return existing;
            }

            // Если нет - создаем новый
            city.IsRecent = true;
            city.LastSearchedAt = DateTime.UtcNow;
            city.IsFavorite = false; // По умолчанию не в избранном
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
        // Если удаляем из истории, просто ставим IsRecent = false
        var city = await _repository.GetCityByIdAsync(cityId, cancellationToken);
        if (city == null) return false;

        city.IsRecent = false;
        await _repository.UpdateCityAsync(city, cancellationToken);
        return true;
    }

    public async Task<bool> RemoveFromHistoryByNameAsync(string cityName, CancellationToken cancellationToken = default)
    {
        var city = await _repository.GetCityByNameAsync(cityName, cancellationToken);
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

    public async Task<int> GetHistoryCountAsync(CancellationToken cancellationToken = default)
    {
        var list = await _repository.GetRecentCitiesAsync(cancellationToken);
        return list.Count;
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