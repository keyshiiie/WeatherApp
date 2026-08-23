using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;
using System.Globalization;
using System.Net.Http.Json;
using WeatherApp.Core.Constants;
using WeatherApp.Core.DTOs;
using WeatherApp.Core.Mappers;
using WeatherApp.Core.Models;
using WeatherApp.Core.Services;

public class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WeatherService> _logger;
    private readonly IWeatherMapper _weatherMapper;
    private readonly ICityMapper _cityMapper;
    private string _language;
    private readonly IApiKeyService _apiKeyService;

    public WeatherService(
        IHttpClientFactory httpClientFactory,
        IWeatherMapper weatherMapper,
        ICityMapper cityMapper,
        ILogger<WeatherService> logger,
        IApiKeyService apiKeyService,
        string language = ApiConstants.DefaultLanguage)
    {
        _httpClient = httpClientFactory.CreateClient("WeatherApi");
        _weatherMapper = weatherMapper;
        _cityMapper = cityMapper;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _apiKeyService = apiKeyService ?? throw new ArgumentNullException(nameof(apiKeyService));
        _language = string.IsNullOrEmpty(language) ? ApiConstants.DefaultLanguage : language;
    }

    public async Task<WeatherData?> GetCurrentWeatherAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default)
    {
        var response = await GetForecastResponseAsync(latitude, longitude, 1, cancellationToken);

        if (response?.Current == null)
        {
            _logger.LogWarning($"No weather data for coordinates: {latitude}, {longitude}");
            return null;
        }

        return _weatherMapper.MapToWeatherDataFromForecast(response);
    }

    public async Task<List<ForecastDay>?> GetForecastAsync(
        double latitude,
        double longitude,
        int days = 5,
        CancellationToken cancellationToken = default)
    {
        var response = await GetForecastResponseAsync(latitude, longitude, days, cancellationToken);

        if (response?.Forecast?.Forecastday == null || !response.Forecast.Forecastday.Any())
        {
            _logger.LogWarning($"No forecast data for coordinates: {latitude}, {longitude}");
            return null;
        }

        return _weatherMapper.MapToForecastDays(response);
    }

    public async Task<(WeatherData? Current, List<ForecastDay>? Forecast)> GetCurrentAndForecastAsync(
        double latitude,
        double longitude,
        int days = 5,
        CancellationToken cancellationToken = default)
    {
        var response = await GetForecastResponseAsync(latitude, longitude, days, cancellationToken);

        if (response == null)
        {
            _logger.LogWarning($"No data for coordinates: {latitude}, {longitude}");
            return (null, null);
        }

        var current = response.Current != null
            ? _weatherMapper.MapToWeatherDataFromForecast(response)
            : null;

        var forecast = response.Forecast?.Forecastday?.Any() == true
            ? _weatherMapper.MapToForecastDays(response)
            : null;

        return (current, forecast);
    }

    public async Task<List<CitySuggestion>?> SearchCitiesAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                return new List<CitySuggestion>();

            var apiKey = await _apiKeyService.GetApiKeyAsync();
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("API key is not available");
                return null;
            }

            var endpoint = $"{ApiConstants.SearchEndpoint}?key={apiKey}&q={Uri.EscapeDataString(query.Trim())}";

            var response = await _httpClient.GetFromJsonAsync<List<SearchResponseDto>>(endpoint, cancellationToken);

            return response?.Any() == true
                ? response.Select(_cityMapper.MapToCitySuggestion).ToList()
                : new List<CitySuggestion>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, $"HTTP error while searching cities for: {query}");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error while searching cities for: {query}");
            return null;
        }
    }

    public void SetLanguage(string languageCode)
    {
        if (string.IsNullOrEmpty(languageCode)) return;
        _language = languageCode;
        _logger.LogInformation($"Language changed to: {languageCode}");
    }

    private string BuildQuery(double latitude, double longitude)
    {
        var latStr = latitude.ToString(CultureInfo.InvariantCulture);
        var lonStr = longitude.ToString(CultureInfo.InvariantCulture);
        return $"{latStr},{lonStr}";
    }

    private string BuildUrl(string endpoint, string query, string apiKey, string additionalParams = "")
    {
        return $"{endpoint}?key={apiKey}&q={Uri.EscapeDataString(query)}&lang={_language}{additionalParams}";
    }

    private async Task<ForecastResponseDto?> GetForecastResponseAsync(
        double latitude,
        double longitude,
        int days,
        CancellationToken cancellationToken)
    {
        try
        {
            days = Math.Clamp(days, 1, 14);
            var query = BuildQuery(latitude, longitude);
            var apiKey = await _apiKeyService.GetApiKeyAsync();
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("API key is not available");
                return null;
            }

            var endpoint = BuildUrl(
                ApiConstants.ForecastEndpoint,
                query,
                apiKey,
                $"&days={days}&aqi=yes");

            return await _httpClient.GetFromJsonAsync<ForecastResponseDto>(endpoint, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, $"HTTP error for coordinates: {latitude}, {longitude}");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error for coordinates: {latitude}, {longitude}");
            return null;
        }
    }
}