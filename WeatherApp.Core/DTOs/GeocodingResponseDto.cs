using System.Text.Json.Serialization;

public class GeocodingResponseDto
{
    [JsonPropertyName("place_id")]
    public long PlaceId { get; set; }

    [JsonPropertyName("licence")]
    public string? Licence { get; set; }

    [JsonPropertyName("osm_type")]
    public string? OsmType { get; set; }

    [JsonPropertyName("osm_id")]
    public long OsmId { get; set; }

    [JsonPropertyName("lat")]
    public string? Lat { get; set; }

    [JsonPropertyName("lon")]
    public string? Lon { get; set; }

    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("address")]
    public AddressDto? Address { get; set; }

    [JsonPropertyName("boundingbox")]
    public List<string>? Boundingbox { get; set; }
}

public class AddressDto
{
    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("town")]
    public string? Town { get; set; }

    [JsonPropertyName("village")]
    public string? Village { get; set; }

    [JsonPropertyName("hamlet")]
    public string? Hamlet { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("region")]
    public string? Region { get; set; }

    [JsonPropertyName("county")]
    public string? County { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("country_code")]
    public string? CountryCode { get; set; }

    [JsonPropertyName("postcode")]
    public string? Postcode { get; set; }

    public string GetCityName()
    {
        var cityName = City ?? Town ?? Village ?? Hamlet;

        if (string.IsNullOrEmpty(cityName))
        {
            cityName = County ?? State ?? Region;
        }

        return cityName ?? "Неизвестное место";
    }

    public string GetFullAddress()
    {
        var parts = new List<string>();
        var cityName = GetCityName();

        if (!string.IsNullOrEmpty(cityName) && cityName != "Неизвестное место")
            parts.Add(cityName);

        if (!string.IsNullOrEmpty(State))
            parts.Add(State);
        else if (!string.IsNullOrEmpty(Region))
            parts.Add(Region);

        if (!string.IsNullOrEmpty(Country))
            parts.Add(Country);

        return parts.Count > 0 ? string.Join(", ", parts) : "Неизвестное место";
    }
}