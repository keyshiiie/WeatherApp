using WeatherApp.Core.DTOs;
using WeatherApp.Core.Entities;
using WeatherApp.Core.Models;
using WeatherApp.Core.Translator;

namespace WeatherApp.Core.Mappers;

public class CityMapper : ICityMapper
{
    public CityEntity? MapToEntity(City? model)
    {
        if (model == null)
            return null;

        return new CityEntity
        {
            Id = model.Id,
            Name = model.Name,
            Region = model.Region,
            Country = model.Country,
            Latitude = model.Latitude,
            Longitude = model.Longitude,
            AddedAt = model.AddedAt,
            IsLastSelected = model.IsLastSelected,
            IsFavorite = model.IsFavorite,
            IsRecent = model.IsRecent,
            LastSearchedAt = model.LastSearchedAt
        };
    }

    public City? MapToModel(CityEntity? entity)
    {
        if (entity == null)
            return null;

        return new City
        {
            Id = entity.Id,
            Name = entity.Name,
            Region = entity.Region,
            Country = entity.Country,
            Latitude = entity.Latitude,
            Longitude = entity.Longitude,
            AddedAt = entity.AddedAt,
            IsLastSelected = entity.IsLastSelected,
            IsFavorite = entity.IsFavorite,
            IsRecent = entity.IsRecent,
            LastSearchedAt = entity.LastSearchedAt
        };
    }
    public CitySuggestion MapToCitySuggestion(SearchResponseDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        var translatedRegion = RegionTranslator.Translate(dto.Region ?? string.Empty);

        return new CitySuggestion
        {
            Id = dto.Id,
            Name = dto.Name ?? string.Empty,
            Region = translatedRegion,
            Country = dto.Country ?? string.Empty,
            Latitude = dto.Lat,
            Longitude = dto.Lon,
            Url = dto.Url ?? string.Empty
        };
    }

    public City MapToCity(GeocodingResponseDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        var address = dto.Address;
        var cityName = address?.GetCityName() ?? "Неизвестное место";

        return new City
        {
            Name = cityName,
            Country = address?.Country ?? "Unknown",
            Region = address?.State ?? address?.Region ?? address?.County,
            Latitude = double.TryParse(dto.Lat, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var lat) ? lat : 0,
            Longitude = double.TryParse(dto.Lon, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var lon) ? lon : 0,
            AddedAt = DateTime.UtcNow,
            IsLastSelected = false
        };
    }
}