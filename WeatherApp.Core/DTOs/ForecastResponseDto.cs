using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WeatherApp.Core.DTOs;
public class ForecastResponseDto
{
    [JsonPropertyName("location")]
    public LocationDto? Location { get; set; }

    [JsonPropertyName("current")]
    public CurrentWeatherDto? Current { get; set; }

    [JsonPropertyName("forecast")]
    public ForecastDto? Forecast { get; set; }
}

public class ForecastDto
{
    [JsonPropertyName("forecastday")]
    public List<ForecastDayDto>? Forecastday { get; set; }
}

public class ForecastDayDto
{
    [JsonPropertyName("date")]
    public string? Date { get; set; }

    [JsonPropertyName("date_epoch")]
    public long DateEpoch { get; set; }

    [JsonPropertyName("day")]
    public DayDataDto? Day { get; set; }

    [JsonPropertyName("astro")]
    public AstroDto? Astro { get; set; }

    [JsonPropertyName("hour")]
    public List<HourlyForecastDto>? Hour { get; set; }
}

public class DayDataDto
{
    [JsonPropertyName("maxtemp_c")]
    public double MaxtempC { get; set; }

    [JsonPropertyName("maxtemp_f")]
    public double MaxtempF { get; set; }

    [JsonPropertyName("mintemp_c")]
    public double MintempC { get; set; }

    [JsonPropertyName("mintemp_f")]
    public double MintempF { get; set; }

    [JsonPropertyName("avgtemp_c")]
    public double AvgtempC { get; set; }

    [JsonPropertyName("avgtemp_f")]
    public double AvgtempF { get; set; }

    [JsonPropertyName("maxwind_mph")]
    public double MaxwindMph { get; set; }

    [JsonPropertyName("maxwind_kph")]
    public double MaxwindKph { get; set; }

    [JsonPropertyName("totalprecip_mm")]
    public double TotalprecipMm { get; set; }

    [JsonPropertyName("totalprecip_in")]
    public double TotalprecipIn { get; set; }

    [JsonPropertyName("avgvis_km")]
    public double AvgvisKm { get; set; }

    [JsonPropertyName("avgvis_miles")]
    public double AvgvisMiles { get; set; }

    [JsonPropertyName("avghumidity")]
    public int Avghumidity { get; set; }

    [JsonPropertyName("condition")]
    public ConditionDto? Condition { get; set; }

    [JsonPropertyName("uv")]
    public double Uv { get; set; }
}

public class AstroDto
{
    [JsonPropertyName("sunrise")]
    public string? Sunrise { get; set; }

    [JsonPropertyName("sunset")]
    public string? Sunset { get; set; }

    [JsonPropertyName("moonrise")]
    public string? Moonrise { get; set; }

    [JsonPropertyName("moonset")]
    public string? Moonset { get; set; }

    [JsonPropertyName("moon_phase")]
    public string? MoonPhase { get; set; }

    [JsonPropertyName("moon_illumination")]
    public int MoonIllumination { get; set; }
}

public class HourlyForecastDto
{
    [JsonPropertyName("time_epoch")]
    public long TimeEpoch { get; set; }

    [JsonPropertyName("time")]
    public string? Time { get; set; }

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

    [JsonPropertyName("windchill_c")]
    public double WindchillC { get; set; }

    [JsonPropertyName("windchill_f")]
    public double WindchillF { get; set; }

    [JsonPropertyName("heatindex_c")]
    public double HeatindexC { get; set; }

    [JsonPropertyName("heatindex_f")]
    public double HeatindexF { get; set; }

    [JsonPropertyName("dewpoint_c")]
    public double DewpointC { get; set; }

    [JsonPropertyName("dewpoint_f")]
    public double DewpointF { get; set; }

    [JsonPropertyName("will_it_rain")]
    public int WillItRain { get; set; }

    [JsonPropertyName("chance_of_rain")]
    public int ChanceOfRain { get; set; }

    [JsonPropertyName("will_it_snow")]
    public int WillItSnow { get; set; }

    [JsonPropertyName("chance_of_snow")]
    public int ChanceOfSnow { get; set; }

    [JsonPropertyName("vis_km")]
    public double VisKm { get; set; }

    [JsonPropertyName("vis_miles")]
    public double VisMiles { get; set; }
}