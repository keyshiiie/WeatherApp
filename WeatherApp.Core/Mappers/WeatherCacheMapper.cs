using System;
using System.Text.Json;
using WeatherApp.Core.Entities;
using WeatherApp.Core.Models;

namespace WeatherApp.Core.Mappers;

public static class WeatherCacheMapper
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    public static WeatherCacheEntity? ToEntity(WeatherData? model, int cityId)
    {
        if (model == null)
            return null;

        return new WeatherCacheEntity
        {
            CityId = cityId,
            CityName = model.CityName,
            Latitude = model.Latitude,
            Longitude = model.Longitude,
            WeatherDataJson = JsonSerializer.Serialize(model, _jsonOptions),
            CachedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30)
        };
    }

    public static WeatherData? ToModel(WeatherCacheEntity? entity)
    {
        if (entity == null || string.IsNullOrEmpty(entity.WeatherDataJson))
            return null;

        try
        {
            var weatherData = JsonSerializer.Deserialize<WeatherData>(
                entity.WeatherDataJson,
                _jsonOptions);

            if (weatherData != null)
            {
                weatherData.IsCached = true;
            }

            return weatherData;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}