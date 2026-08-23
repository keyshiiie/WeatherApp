using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net.Http.Json;
using WeatherApp.Core.Constants;
using WeatherApp.Core.DTOs;
using WeatherApp.Core.Mappers;
using WeatherApp.Core.Models;
using WeatherApp.Core.Results;

namespace WeatherApp.Core.Services;

public class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WeatherService> _logger;
    private readonly IWeatherMapper _weatherMapper;
    private readonly ICityMapper _cityMapper;
    private readonly IApiKeyService _apiKeyService;
    private string _language;

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

    public async Task<Result<WeatherData>> GetCurrentWeatherAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Валидация координат
            if (latitude < -90 || latitude > 90)
                return Result.Failure<WeatherData>(new ValidationError("Некорректная широта. Допустимый диапазон: -90 до 90"));

            if (longitude < -180 || longitude > 180)
                return Result.Failure<WeatherData>(new ValidationError("Некорректная долгота. Допустимый диапазон: -180 до 180"));

            var responseResult = await GetForecastResponseAsync(latitude, longitude, 1, cancellationToken);

            if (responseResult.IsFailure)
                return Result.Failure<WeatherData>(responseResult.Error!);

            var response = responseResult.Value;
            if (response?.Current == null)
            {
                _logger.LogWarning($"No weather data for coordinates: {latitude}, {longitude}");
                return Result.Failure<WeatherData>(new NotFoundError("Weather", $"{latitude},{longitude}"));
            }

            var weatherData = _weatherMapper.MapToWeatherDataFromForecast(response);
            if (weatherData == null)
            {
                return Result.Failure<WeatherData>(new UnknownError("Failed to map weather data"));
            }

            return Result.Success(weatherData);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, $"Request cancelled for coordinates: {latitude}, {longitude}");
            return Result.Failure<WeatherData>(new TimeoutError());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error for coordinates: {latitude}, {longitude}");
            return Result.Failure<WeatherData>(new UnknownError($"Unexpected error: {ex.Message}", ex));
        }
    }

    public async Task<Result<List<ForecastDay>>> GetForecastAsync(
        double latitude,
        double longitude,
        int days = 5,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Валидация
            if (latitude < -90 || latitude > 90)
                return Result.Failure<List<ForecastDay>>(new ValidationError("Некорректная широта"));

            if (longitude < -180 || longitude > 180)
                return Result.Failure<List<ForecastDay>>(new ValidationError("Некорректная долгота"));

            if (days < 1 || days > 14)
                return Result.Failure<List<ForecastDay>>(new ValidationError("Количество дней должно быть от 1 до 14"));

            var responseResult = await GetForecastResponseAsync(latitude, longitude, days, cancellationToken);

            if (responseResult.IsFailure)
                return Result.Failure<List<ForecastDay>>(responseResult.Error!);

            var response = responseResult.Value;
            if (response?.Forecast?.Forecastday == null || !response.Forecast.Forecastday.Any())
            {
                _logger.LogWarning($"No forecast data for coordinates: {latitude}, {longitude}");
                return Result.Failure<List<ForecastDay>>(new NotFoundError("Forecast", $"{latitude},{longitude}"));
            }

            var forecast = _weatherMapper.MapToForecastDays(response);
            if (forecast == null || !forecast.Any())
            {
                return Result.Failure<List<ForecastDay>>(new UnknownError("Failed to map forecast data"));
            }

            return Result.Success(forecast);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, $"Request cancelled for coordinates: {latitude}, {longitude}");
            return Result.Failure<List<ForecastDay>>(new TimeoutError());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error for coordinates: {latitude}, {longitude}");
            return Result.Failure<List<ForecastDay>>(new UnknownError($"Unexpected error: {ex.Message}", ex));
        }
    }

    public async Task<Result<(WeatherData Current, List<ForecastDay> Forecast)>> GetCurrentAndForecastAsync(
        double latitude,
        double longitude,
        int days = 5,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Валидация
            if (latitude < -90 || latitude > 90)
                return Result.Failure<(WeatherData, List<ForecastDay>)>(new ValidationError("Некорректная широта"));

            if (longitude < -180 || longitude > 180)
                return Result.Failure<(WeatherData, List<ForecastDay>)>(new ValidationError("Некорректная долгота"));

            if (days < 1 || days > 14)
                return Result.Failure<(WeatherData, List<ForecastDay>)>(new ValidationError("Количество дней должно быть от 1 до 14"));

            var responseResult = await GetForecastResponseAsync(latitude, longitude, days, cancellationToken);

            if (responseResult.IsFailure)
                return Result.Failure<(WeatherData, List<ForecastDay>)>(responseResult.Error!);

            var response = responseResult.Value;
            if (response == null)
            {
                _logger.LogWarning($"No data for coordinates: {latitude}, {longitude}");
                return Result.Failure<(WeatherData, List<ForecastDay>)>(new NotFoundError("Weather", $"{latitude},{longitude}"));
            }

            var current = response.Current != null
                ? _weatherMapper.MapToWeatherDataFromForecast(response)
                : null;

            var forecast = response.Forecast?.Forecastday?.Any() == true
                ? _weatherMapper.MapToForecastDays(response)
                : null;

            if (current == null && (forecast == null || !forecast.Any()))
            {
                return Result.Failure<(WeatherData, List<ForecastDay>)>(new NotFoundError("Weather", $"{latitude},{longitude}"));
            }

            return Result.Success((current!, forecast!));
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, $"Request cancelled for coordinates: {latitude}, {longitude}");
            return Result.Failure<(WeatherData, List<ForecastDay>)>(new TimeoutError());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error for coordinates: {latitude}, {longitude}");
            return Result.Failure<(WeatherData, List<ForecastDay>)>(new UnknownError($"Unexpected error: {ex.Message}", ex));
        }
    }

    public async Task<Result<List<CitySuggestion>>> SearchCitiesAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                return Result.Success(new List<CitySuggestion>());

            var apiKeyResult = await _apiKeyService.GetApiKeyAsync();
            if (apiKeyResult.IsFailure)
                return Result.Failure<List<CitySuggestion>>(apiKeyResult.Error!);

            var apiKey = apiKeyResult.Value!;
            var endpoint = $"{ApiConstants.SearchEndpoint}?key={apiKey}&q={Uri.EscapeDataString(query.Trim())}";

            var response = await _httpClient.GetFromJsonAsync<List<SearchResponseDto>>(endpoint, cancellationToken);

            if (response == null || !response.Any())
                return Result.Success(new List<CitySuggestion>());

            var suggestions = response
                .Select(_cityMapper.MapToCitySuggestion)
                .Where(s => s != null)
                .Select(s => s!)
                .ToList();

            return Result.Success(suggestions);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, $"HTTP error while searching cities for: {query}");
            return Result.Failure<List<CitySuggestion>>(new NetworkError($"Network error while searching: {ex.Message}"));
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, $"Search cancelled for query: {query}");
            return Result.Failure<List<CitySuggestion>>(new TimeoutError());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error while searching cities for: {query}");
            return Result.Failure<List<CitySuggestion>>(new UnknownError($"Failed to search cities: {ex.Message}", ex));
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

    private async Task<Result<ForecastResponseDto?>> GetForecastResponseAsync(
        double latitude,
        double longitude,
        int days,
        CancellationToken cancellationToken)
    {
        try
        {
            days = Math.Clamp(days, 1, 14);
            var query = BuildQuery(latitude, longitude);

            var apiKeyResult = await _apiKeyService.GetApiKeyAsync();
            if (apiKeyResult.IsFailure)
                return Result.Failure<ForecastResponseDto?>(apiKeyResult.Error!);

            var apiKey = apiKeyResult.Value!;
            var endpoint = BuildUrl(
                ApiConstants.ForecastEndpoint,
                query,
                apiKey,
                $"&days={days}&aqi=yes");

            var response = await _httpClient.GetFromJsonAsync<ForecastResponseDto>(endpoint, cancellationToken);
            return Result.Success(response);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, $"HTTP error for coordinates: {latitude}, {longitude}");
            return Result.Failure<ForecastResponseDto?>(new ApiError("HTTP request failed", 500, ex.Message));
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, $"Request cancelled for coordinates: {latitude}, {longitude}");
            return Result.Failure<ForecastResponseDto?>(new TimeoutError());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error for coordinates: {latitude}, {longitude}");
            return Result.Failure<ForecastResponseDto?>(new UnknownError($"Unexpected error: {ex.Message}", ex));
        }
    }
}