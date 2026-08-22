using WeatherApp.Core.Models;

namespace WeatherApp.Core.Repositories
{
    public interface IHistoryRepository
    {
        Task<List<City>> GetHistoryAsync(CancellationToken cancellationToken = default);
        Task<City> AddInHistoryAsync(City city, CancellationToken cancellationToken = default);
        Task<bool> RemoveFromHistoryAsync(int cityId, CancellationToken cancellationToken = default);
        Task<bool> IsRecentAsync(string cityName, CancellationToken cancellationToken = default);
        Task ClearHistoryAsync(CancellationToken cancellationToken = default);
    }
}
