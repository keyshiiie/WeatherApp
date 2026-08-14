using System.Text.Json.Serialization;

namespace WeatherApp.Core.DTOs;

public class SearchResponseDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("region")]
    public string? Region { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("lat")]
    public double Lat { get; set; }

    [JsonPropertyName("lon")]
    public double Lon { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonIgnore]
    public string? TranslatedRegion { get; set; }

    [JsonIgnore]
    public string FullDisplayName => string.IsNullOrEmpty(TranslatedRegion)
        ? $"{Name}, {Country}"
        : $"{Name}, {TranslatedRegion}, {Country}";

    [JsonIgnore]
    public string ShortDisplayName => $"{Name}, {Country}";
}