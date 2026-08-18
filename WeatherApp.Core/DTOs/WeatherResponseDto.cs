using System.Text.Json.Serialization;

namespace WeatherApp.Core.DTOs;

public class WeatherResponseDto
{
    [JsonPropertyName("location")]
    public LocationDto? Location { get; set; }

    [JsonPropertyName("current")]
    public CurrentWeatherDto? Current { get; set; }
}

public class LocationDto
{
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

    [JsonPropertyName("tz_id")]
    public string? TzId { get; set; }

    [JsonPropertyName("localtime_epoch")]
    public long LocaltimeEpoch { get; set; }

    [JsonPropertyName("localtime")]
    public string? Localtime { get; set; }
}

public class CurrentWeatherDto
{
    [JsonPropertyName("last_updated_epoch")]
    public long LastUpdatedEpoch { get; set; }

    [JsonPropertyName("last_updated")]
    public string? LastUpdated { get; set; }

    [JsonPropertyName("temp_c")]
    public double TempC { get; set; }

    [JsonPropertyName("temp_f")]
    public double TempF { get; set; }

    [JsonPropertyName("is_day")]
    public int IsDay { get; set; }

    [JsonPropertyName("condition")]
    public ConditionDto? Condition { get; set; }

    [JsonPropertyName("wind_mph")]
    public double WindMph { get; set; }

    [JsonPropertyName("wind_kph")]
    public double WindKph { get; set; }

    [JsonPropertyName("wind_degree")]
    public int WindDegree { get; set; }

    [JsonPropertyName("wind_dir")]
    public string? WindDir { get; set; }

    [JsonPropertyName("pressure_mb")]
    public double PressureMb { get; set; }

    [JsonPropertyName("pressure_in")]
    public double PressureIn { get; set; }

    [JsonPropertyName("precip_mm")]
    public double PrecipMm { get; set; }

    [JsonPropertyName("precip_in")]
    public double PrecipIn { get; set; }

    [JsonPropertyName("humidity")]
    public int Humidity { get; set; }

    [JsonPropertyName("cloud")]
    public int Cloud { get; set; }

    [JsonPropertyName("feelslike_c")]
    public double FeelslikeC { get; set; }

    [JsonPropertyName("feelslike_f")]
    public double FeelslikeF { get; set; }

    [JsonPropertyName("vis_km")]
    public double VisKm { get; set; }

    [JsonPropertyName("vis_miles")]
    public double VisMiles { get; set; }

    [JsonPropertyName("uv")]
    public double Uv { get; set; }

    [JsonPropertyName("gust_mph")]
    public double GustMph { get; set; }

    [JsonPropertyName("gust_kph")]
    public double GustKph { get; set; }

    [JsonPropertyName("air_quality")]
    public AirQualityDto? AirQuality { get; set; }
}

public class ConditionDto
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    [JsonPropertyName("code")]
    public int Code { get; set; }
}

public class AirQualityDto
{
    [JsonPropertyName("co")]
    public double Co { get; set; }

    [JsonPropertyName("no2")]
    public double No2 { get; set; }

    [JsonPropertyName("o3")]
    public double O3 { get; set; }

    [JsonPropertyName("so2")]
    public double So2 { get; set; }

    [JsonPropertyName("pm2_5")]
    public double Pm25 { get; set; }

    [JsonPropertyName("pm10")]
    public double Pm10 { get; set; }

    [JsonPropertyName("us-epa-index")]
    public int UsEpaIndex { get; set; }

    [JsonPropertyName("gb-defra-index")]
    public int GbDefraIndex { get; set; }
}