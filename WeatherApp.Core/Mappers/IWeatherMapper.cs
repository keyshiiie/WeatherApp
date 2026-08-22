using System;
using System.Collections.Generic;
using System.Text;
using WeatherApp.Core.DTOs;
using WeatherApp.Core.Models;

namespace WeatherApp.Core.Mappers
{
    public interface IWeatherMapper
    {
        WeatherData MapToWeatherData(WeatherResponseDto dto);
        WeatherData MapToWeatherDataFromForecast(ForecastResponseDto dto);
        List<ForecastDay> MapToForecastDays(ForecastResponseDto dto);
    }
}
