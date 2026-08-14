using System;
using System.Collections.Generic;
using System.Text;
using WeatherApp.Core.Models;
using WeatherApp.Core.Repositories;

namespace WeatherApp.Core.Services
{
    public interface ICityService : IFavoritesRepository, IHistoryRepository
    {
        Task<City?> GetBestCityAsync(CancellationToken cancellationToken = default);
    }
}
