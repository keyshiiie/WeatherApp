using System;
using System.Collections.Generic;
using System.Text;
using WeatherApp.Core.Models;

namespace WeatherApp.Core.Services
{
    public interface IGeolocationService
    {
        Task<City?> GetCurrentLocationAsync(CancellationToken cancellationToken = default);
        Task<string?> GetCityNameFromCoordinatesAsync(double latitude, double longitude, CancellationToken cancellationToken = default);
        Task<bool> CheckLocationPermissionAsync();
        Task<bool> RequestLocationPermissionAsync();
        
    }
}
