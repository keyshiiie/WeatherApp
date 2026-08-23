using WeatherApp.Core.Models;
using WeatherApp.Core.Results;

namespace WeatherApp.Core.Repositories
{
    public interface IHistoryRepository
    {
        Task<Result<List<City>>> GetHistoryAsync(CancellationToken cancellationToken = default);
        Task<Result<City>> AddInHistoryAsync(City city, CancellationToken cancellationToken = default);
        Task<Result<bool>> RemoveFromHistoryAsync(int cityId, CancellationToken cancellationToken = default);
        Task<Result<bool>> IsRecentAsync(string cityName, CancellationToken cancellationToken = default);
        Task<Result> ClearHistoryAsync(CancellationToken cancellationToken = default);
    }
}
