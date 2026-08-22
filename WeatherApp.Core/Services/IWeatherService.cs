using WeatherApp.Core.Models;

namespace WeatherApp.Core.Services
{
    public interface IWeatherService
    {
        Task<WeatherData?> GetCurrentWeatherAsync(double latitude, double longitude, CancellationToken cancellationToken = default);
        Task<List<ForecastDay>?> GetForecastAsync(double latitude, double longitude, int days = 5, CancellationToken cancellationToken = default);
        Task<List<CitySuggestion>?> SearchCitiesAsync(string query, CancellationToken cancellationToken = default);
        Task<(WeatherData? Current, List<ForecastDay>? Forecast)> GetCurrentAndForecastAsync(
        double latitude,
        double longitude,
        int days = 5,
        CancellationToken cancellationToken = default);
        void SetLanguage(string languageCode);

    }
}
