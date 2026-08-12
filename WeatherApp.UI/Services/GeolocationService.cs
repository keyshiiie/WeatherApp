using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using WeatherApp.Core.Configuration;
using WeatherApp.Core.Constants;
using WeatherApp.Core.DTOs;
using WeatherApp.Core.Models;
using WeatherApp.Core.Services;

namespace WeatherApp.UI.Services
{
    public class GeolocationService : IGeolocationService
    {
        private readonly HttpClient _httpClient;
        private readonly ApiSettings _apiSettings;
        private readonly ILogger<GeolocationService> _logger;

        public GeolocationService(
        HttpClient httpClient,
        IOptions<ApiSettings> apiSettings,
        ILogger<GeolocationService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _apiSettings = apiSettings?.Value ?? throw new ArgumentNullException(nameof(apiSettings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (!string.IsNullOrEmpty(_apiSettings.NominatimBaseUrl))
            {
                _httpClient.BaseAddress = new Uri(_apiSettings.NominatimBaseUrl);
            }

            _httpClient.DefaultRequestHeaders.Add("User-Agent", ApiConstants.NominatimUserAgent);
        }

        public async Task<City?> GetCurrentLocationAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var hasPermission = await RequestLocationPermissionAsync();
                if (!hasPermission)
                {
                    _logger.LogWarning("Location permission denied");
                    return null;
                }

                var coordinates = await GetCoordinatesAsync(cancellationToken);
                if (coordinates == null)
                {
                    _logger.LogWarning("Failed to get coordinates");
                    return null;
                }

                var address = await GetAddressFromCoordinatesAsync(
                    coordinates.Value.Latitude,
                    coordinates.Value.Longitude,
                    cancellationToken);

                if (address == null)
                {
                    _logger.LogWarning("Failed to get address from coordinates");
                    return null;
                }

                var cityName = address.GetCityName();
                if (string.IsNullOrEmpty(cityName))
                {
                    _logger.LogWarning($"City name not found for coordinates");
                    return null;
                }

                return new City
                {
                    Name = cityName,
                    Country = address.Country ?? "Unknown",
                    Region = address.State ?? address.Region,
                    Latitude = coordinates.Value.Latitude,
                    Longitude = coordinates.Value.Longitude,
                    AddedAt = DateTime.UtcNow,
                    IsLastSelected = false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current location");
                return null;
            }
        }

        private async Task<AddressDto?> GetAddressFromCoordinatesAsync(
            double latitude,
            double longitude,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var latStr = latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
                var lonStr = longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
                var url = $"reverse?format={ApiConstants.NominatimFormat}&lat={latStr}&lon={lonStr}&zoom=10";

                var response = await _httpClient.GetFromJsonAsync<GeocodingResponseDto>(url, cancellationToken);
                return response?.Address;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting address from coordinates: {latitude}, {longitude}");
                return null;
            }
        }

        public async Task<string?> GetCityNameFromCoordinatesAsync(
            double latitude,
            double longitude,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var latStr = latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
                var lonStr = longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
                var url = $"reverse?format={ApiConstants.NominatimFormat}&lat={latStr}&lon={lonStr}";

                var response = await _httpClient.GetFromJsonAsync<GeocodingResponseDto>(url, cancellationToken);
                if (response?.Address == null)
                {
                    _logger.LogWarning($"No address found for coordinates: {latitude}, {longitude}");
                    return null;
                }

                var cityName = response.Address.GetCityName();
                if (string.IsNullOrEmpty(cityName))
                {
                    _logger.LogWarning($"City name not found in address: {latitude}, {longitude}");
                    return null;
                }

                _logger.LogInformation($"City found: {cityName} for coordinates: {latitude}, {longitude}");
                return cityName;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"HTTP error getting city name for coordinates: {latitude}, {longitude}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error getting city name for coordinates: {latitude}, {longitude}");
                return null;
            }
        }

        private async Task<(double Latitude, double Longitude)?> GetCoordinatesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var location = await Geolocation.Default.GetLocationAsync(
                new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Medium, 
                    Timeout = TimeSpan.FromSeconds(30)
                },
                cancellationToken);

                if (location == null)
                {
                    _logger.LogWarning("Geolocation returned null");
                    return null;
                }

                _logger.LogInformation($"Location obtained: {location.Latitude}, {location.Longitude}");
                return (location.Latitude, location.Longitude);
            }
            catch (FeatureNotSupportedException ex)
            {
                _logger.LogError(ex, "Geolocation is not supported on this device");
                return null;
            }
            catch (PermissionException ex)
            {
                _logger.LogError(ex, "Location permission denied");
                return null;
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Geolocation request timed out");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting coordinates");
                return null;
            }
        }

        public async Task<bool> CheckLocationPermissionAsync()
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                return status == PermissionStatus.Granted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking location permission");
                return false;
            }
        }

        public async Task<bool> RequestLocationPermissionAsync()
        {
            try
            {
                if (await CheckLocationPermissionAsync())
                {
                    return true;
                }

                if (DeviceInfo.Current.Platform == DevicePlatform.WinUI)
                {
                    var page = Shell.Current?.CurrentPage;
                    if (page == null)
                        return false;

                    var result = await page.DisplayAlertAsync(
                        "📍 Разрешение на геолокацию",
                        "Для определения вашего местоположения необходимо включить доступ к геолокации в настройках Windows.\n\n" +
                        "Перейдите в: Настройки > Конфиденциальность > Местоположение и включите доступ для этого приложения.",
                        "Открыть настройки",
                        "Отмена");

                    if (result)
                    {
                        await Launcher.Default.OpenAsync("ms-settings:privacy-location");
                    }
                    return false;
                }

                var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

                if (status == PermissionStatus.Granted)
                {
                    _logger.LogInformation("Location permission granted");
                    return true;
                }

                if (status == PermissionStatus.Denied)
                {
                    _logger.LogWarning("Location permission denied by user");
                    return false;
                }

                if (status == PermissionStatus.Restricted)
                {
                    _logger.LogWarning("Location permission restricted");
                    return false;
                }

                if (status == PermissionStatus.Denied)
                {
                    _logger.LogWarning("Location permission permanently denied. Please enable in settings.");
                    return false;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting location permission");
                return false;
            }
        }
    }
}
