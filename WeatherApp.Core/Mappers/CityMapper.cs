using WeatherApp.Core.Entities;
using WeatherApp.Core.Models;

namespace WeatherApp.Core.Mappers;

public static class CityMapper
{
    public static CityEntity? ToEntity(City? model)
    {
        if (model == null)
            return null;

        return new CityEntity
        {
            Id = model.Id,
            Name = model.Name,
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

    public static City? ToModel(CityEntity? entity)
    {
        if (entity == null)
            return null;

        return new City
        {
            Id = entity.Id,
            Name = entity.Name,
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
}