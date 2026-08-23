using WeatherApp.Core.Models;
using WeatherApp.Core.Results;

namespace WeatherApp.Core.Services;

public interface IWeatherService
{
    Task<Result<WeatherData>> GetCurrentWeatherAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default);

    Task<Result<List<ForecastDay>>> GetForecastAsync(
        double latitude,
        double longitude,
        int days = 5,
        CancellationToken cancellationToken = default);

    Task<Result<List<CitySuggestion>>> SearchCitiesAsync(
        string query,
        CancellationToken cancellationToken = default);

    Task<Result<(WeatherData Current, List<ForecastDay> Forecast)>> GetCurrentAndForecastAsync(
        double latitude,
        double longitude,
        int days = 5,
        CancellationToken cancellationToken = default);

    void SetLanguage(string languageCode);
}