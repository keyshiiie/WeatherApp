using WeatherApp.Core.DTOs;
using WeatherApp.Core.Entities;
using WeatherApp.Core.Models;

namespace WeatherApp.Core.Mappers
{
    public interface ICityMapper
    {
        CityEntity? MapToEntity(City? model);
        City? MapToModel(CityEntity? entity);
        CitySuggestion MapToCitySuggestion(SearchResponseDto dto);
        City MapToCity(GeocodingResponseDto dto);
    }
}
