using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;
using System.Net.Http.Json;
using WeatherApp.Core.Constants;
using WeatherApp.Core.DTOs;
using WeatherApp.Core.Mappers;
using WeatherApp.Core.Models;

namespace WeatherApp.Core.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<WeatherService> _logger;
        private string _language;

        public WeatherService(
            IHttpClientFactory httpClientFactory,
            ILogger<WeatherService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("WeatherApi");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _language = ApiConstants.DefaultLanguage;
        }

        public WeatherService(
            IHttpClientFactory httpClientFactory,
            ILogger<WeatherService> logger,
            string language = ApiConstants.DefaultLanguage)
        {
            _httpClient = httpClientFactory.CreateClient("WeatherApi");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _language = string.IsNullOrEmpty(language) ? ApiConstants.DefaultLanguage : language;
        }

        private async Task<string> GetApiKeyAsync()
        {
            var key = await SecureStorage.GetAsync("weather_api_key");
            if (string.IsNullOrEmpty(key))
            {
                _logger.LogWarning("API Key not found in SecureStorage!");
            }
            return key ?? string.Empty;
        }

        private string BuildUrl(string endpoint, string query, string apiKey, string additionalParams = "")
        {
            return $"{endpoint}?key={apiKey}&q={Uri.EscapeDataString(query)}&lang={_language}{additionalParams}";
        }

        public async Task<WeatherData?> GetCurrentWeatherAsync(
            string cityName,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cityName))
                {
                    _logger.LogWarning("City name is empty");
                    return null;
                }

                var apiKey = await GetApiKeyAsync();
                if (string.IsNullOrEmpty(apiKey)) return null;

                var endpoint = BuildUrl(
                    ApiConstants.CurrentWeatherEndpoint,
                    cityName,
                    apiKey,
                    "&aqi=yes");

                var response = await _httpClient.GetFromJsonAsync<WeatherResponseDto>(
                    endpoint,
                    cancellationToken);

                if (response == null || response.Current == null)
                {
                    _logger.LogWarning($"No weather data received for city: {cityName}");
                    return null;
                }

                return WeatherDtoMapper.MapToWeatherData(response);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"HTTP error while fetching weather for city: {cityName}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error while fetching weather for city: {cityName}");
                return null;
            }
        }

        public async Task<WeatherData?> GetCurrentWeatherAsync(
            double latitude,
            double longitude,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var latStr = latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
                var lonStr = longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
                var query = $"{latStr},{lonStr}";

                var apiKey = await GetApiKeyAsync();
                if (string.IsNullOrEmpty(apiKey)) return null;

                var endpoint = BuildUrl(
                    ApiConstants.CurrentWeatherEndpoint,
                    query,
                    apiKey,
                    "&aqi=yes");

                var response = await _httpClient.GetFromJsonAsync<WeatherResponseDto>(
                    endpoint,
                    cancellationToken);

                if (response == null || response.Current == null)
                {
                    _logger.LogWarning($"No weather data received for coordinates: {latitude}, {longitude}");
                    return null;
                }

                return WeatherDtoMapper.MapToWeatherData(response);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"HTTP error while fetching weather for coordinates: {latitude}, {longitude}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error while fetching weather for coordinates: {latitude}, {longitude}");
                return null;
            }
        }

        public async Task<List<ForecastDay>?> GetForecastAsync(
            string cityName,
            int days = 5,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cityName))
                {
                    _logger.LogWarning("City name is empty");
                    return null;
                }

                days = Math.Clamp(days, 1, 14);
                var apiKey = await GetApiKeyAsync();
                if (string.IsNullOrEmpty(apiKey)) return null;

                var endpoint = BuildUrl(
                    ApiConstants.ForecastEndpoint,
                    cityName,
                    apiKey,
                    $"&days={days}&aqi=yes");

                var response = await _httpClient.GetFromJsonAsync<ForecastResponseDto>(
                    endpoint,
                    cancellationToken);

                if (response?.Forecast?.Forecastday == null || !response.Forecast.Forecastday.Any())
                {
                    _logger.LogWarning($"No forecast data received for city: {cityName}");
                    return null;
                }

                return WeatherDtoMapper.MapToForecastDays(response);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"HTTP error while fetching forecast for city: {cityName}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error while fetching forecast for city: {cityName}");
                return null;
            }
        }

        public async Task<List<ForecastDay>?> GetForecastAsync(
            double latitude,
            double longitude,
            int days = 5,
            CancellationToken cancellationToken = default)
        {
            try
            {
                days = Math.Clamp(days, 1, 14);
                var latStr = latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
                var lonStr = longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
                var query = $"{latStr},{lonStr}";

                var apiKey = await GetApiKeyAsync();
                if (string.IsNullOrEmpty(apiKey)) return null;

                var endpoint = BuildUrl(
                    ApiConstants.ForecastEndpoint,
                    query,
                    apiKey,
                    $"&days={days}&aqi=yes");

                var response = await _httpClient.GetFromJsonAsync<ForecastResponseDto>(
                    endpoint,
                    cancellationToken);

                if (response?.Forecast?.Forecastday == null || !response.Forecast.Forecastday.Any())
                {
                    _logger.LogWarning($"No forecast data received for coordinates: {latitude}, {longitude}");
                    return null;
                }

                return WeatherDtoMapper.MapToForecastDays(response);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"HTTP error while fetching forecast for coordinates: {latitude}, {longitude}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error while fetching forecast for coordinates: {latitude}, {longitude}");
                return null;
            }
        }

        public async Task<List<CitySuggestion>?> SearchCitiesAsync(
            string query,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                {
                    return new List<CitySuggestion>();
                }

                var apiKey = await GetApiKeyAsync();
                if (string.IsNullOrEmpty(apiKey)) return null;

                var endpoint = $"{ApiConstants.SearchEndpoint}?key={apiKey}&q={Uri.EscapeDataString(query.Trim())}";

                var response = await _httpClient.GetFromJsonAsync<List<SearchResponseDto>>(
                    endpoint,
                    cancellationToken);

                if (response == null || !response.Any())
                {
                    return new List<CitySuggestion>();
                }

                return response.Select(WeatherDtoMapper.MapToCitySuggestion).ToList();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"HTTP error while searching cities for query: {query}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error while searching cities for query: {query}");
                return null;
            }
        }

        public async Task<(WeatherData? Current, List<ForecastDay>? Forecast)> GetCurrentAndForecastAsync(
            string cityName,
            int days = 5,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cityName))
                {
                    _logger.LogWarning("City name is empty");
                    return (null, null);
                }

                days = Math.Clamp(days, 1, 14);
                var apiKey = await GetApiKeyAsync();
                if (string.IsNullOrEmpty(apiKey)) return (null, null);

                var endpoint = BuildUrl(
                    ApiConstants.ForecastEndpoint,
                    cityName,
                    apiKey,
                    $"&days={days}&aqi=yes");

                var response = await _httpClient.GetFromJsonAsync<ForecastResponseDto>(
                    endpoint,
                    cancellationToken);

                if (response == null)
                {
                    _logger.LogWarning($"No data received for city: {cityName}");
                    return (null, null);
                }

                var current = response.Current != null ? WeatherDtoMapper.MapToWeatherDataFromForecast(response) : null;
                var forecast = response.Forecast?.Forecastday != null && response.Forecast.Forecastday.Any()
                    ? WeatherDtoMapper.MapToForecastDays(response)
                    : null;

                return (current, forecast);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"HTTP error while fetching weather and forecast for city: {cityName}");
                return (null, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error while fetching weather and forecast for city: {cityName}");
                return (null, null);
            }
        }

        public async Task<(WeatherData? Current, List<ForecastDay>? Forecast)> GetCurrentAndForecastAsync(
            double latitude,
            double longitude,
            int days = 5,
            CancellationToken cancellationToken = default)
        {
            try
            {
                days = Math.Clamp(days, 1, 14);
                var latStr = latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
                var lonStr = longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
                var query = $"{latStr},{lonStr}";

                var apiKey = await GetApiKeyAsync();
                if (string.IsNullOrEmpty(apiKey)) return (null, null);

                var endpoint = BuildUrl(
                    ApiConstants.ForecastEndpoint,
                    query,
                    apiKey,
                    $"&days={days}&aqi=yes");

                var response = await _httpClient.GetFromJsonAsync<ForecastResponseDto>(
                    endpoint,
                    cancellationToken);

                if (response == null)
                {
                    _logger.LogWarning($"No data received for coordinates: {latitude}, {longitude}");
                    return (null, null);
                }

                var current = response.Current != null ? WeatherDtoMapper.MapToWeatherDataFromForecast(response) : null;
                var forecast = response.Forecast?.Forecastday != null && response.Forecast.Forecastday.Any()
                    ? WeatherDtoMapper.MapToForecastDays(response)
                    : null;

                return (current, forecast);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"HTTP error while fetching weather and forecast for coordinates: {latitude}, {longitude}");
                return (null, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error while fetching weather and forecast for coordinates: {latitude}, {longitude}");
                return (null, null);
            }
        }

        public void SetLanguage(string languageCode)
        {
            if (string.IsNullOrEmpty(languageCode)) return;
            _language = languageCode;
            _logger.LogInformation($"Language changed to: {languageCode}");
        }
    }
}