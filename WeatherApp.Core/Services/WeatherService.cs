using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using WeatherApp.Core.Configuration;
using WeatherApp.Core.Constants;
using WeatherApp.Core.DTOs;
using WeatherApp.Core.Models;
using WeatherApp.Core.Mappers;

namespace WeatherApp.Core.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly ApiSettings _apiSettings;
        private readonly ILogger<WeatherService> _logger;

        public WeatherService(
            HttpClient httpClient,
            IOptions<ApiSettings> apiSettings,
            ILogger<WeatherService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _apiSettings = apiSettings?.Value ?? throw new ArgumentNullException(nameof(apiSettings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Настройка базового URL
            if (!string.IsNullOrEmpty(_apiSettings.WeatherApiBaseUrl))
            {
                _httpClient.BaseAddress = new Uri(_apiSettings.WeatherApiBaseUrl);
            }
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

                var endpoint = $"{ApiConstants.CurrentWeatherEndpoint}?key={_apiSettings.WeatherApiKey}&q={Uri.EscapeDataString(cityName)}&aqi=yes";

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
                var query = $"{latitude},{longitude}";
                var endpoint = $"{ApiConstants.CurrentWeatherEndpoint}?key={_apiSettings.WeatherApiKey}&q={query}&aqi=yes";

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
                var endpoint = $"{ApiConstants.ForecastEndpoint}?key={_apiSettings.WeatherApiKey}&q={Uri.EscapeDataString(cityName)}&days={days}&aqi=yes";

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
                var query = $"{latitude},{longitude}";
                var endpoint = $"{ApiConstants.ForecastEndpoint}?key={_apiSettings.WeatherApiKey}&q={query}&days={days}&aqi=yes";

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

                var endpoint = $"{ApiConstants.SearchEndpoint}?key={_apiSettings.WeatherApiKey}&q={Uri.EscapeDataString(query)}";

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
                var endpoint = $"{ApiConstants.ForecastEndpoint}?key={_apiSettings.WeatherApiKey}&q={Uri.EscapeDataString(cityName)}&days={days}&aqi=yes";

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
                var query = $"{latitude},{longitude}";
                var endpoint = $"{ApiConstants.ForecastEndpoint}?key={_apiSettings.WeatherApiKey}&q={query}&days={days}&aqi=yes";

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
    }
}