namespace WeatherApp.Core.Constants;

public static class ApiConstants
{
    // Базовые URL будут загружаться из конфигурации
    public const string WeatherApiBaseUrl = "https://api.weatherapi.com/v1/";
    public const string NominatimBaseUrl = "https://nominatim.openstreetmap.org/";

    // Эндпоинты
    public const string CurrentWeatherEndpoint = "current.json";
    public const string ForecastEndpoint = "forecast.json";
    public const string SearchEndpoint = "search.json";

    // Параметры Nominatim
    public const string NominatimFormat = "json";
    public const string NominatimUserAgent = "WeatherApp/1.0";

    // Параметры по умолчанию
    public const int DefaultForecastDays = 5;
    public const int CacheDurationMinutes = 30;
}