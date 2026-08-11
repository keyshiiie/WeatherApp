namespace WeatherApp.Core.Configuration;

public class ApiSettings
{
    public string WeatherApiKey { get; set; } = string.Empty;
    public string WeatherApiBaseUrl { get; set; } = string.Empty;
    public string NominatimBaseUrl { get; set; } = string.Empty;
}