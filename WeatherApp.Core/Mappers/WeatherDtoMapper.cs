using System;
using System.Collections.Generic;
using System.Text;
using WeatherApp.Core.DTOs;
using WeatherApp.Core.Models;
using WeatherApp.Core.Translator;

namespace WeatherApp.Core.Mappers
{
    public class WeatherDtoMapper : IWeatherMapper
    {
        private readonly int _rainThresholdPercent;

        public WeatherDtoMapper(int rainThresholdPercent = 30)
        {
            _rainThresholdPercent = rainThresholdPercent;
        }

        public WeatherData MapToWeatherData(WeatherResponseDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);
            return CreateWeatherData(dto.Location, dto.Current);
        }

        public WeatherData MapToWeatherDataFromForecast(ForecastResponseDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var weatherData = CreateWeatherData(dto.Location, dto.Current);

            var todayForecast = dto.Forecast?.Forecastday?.FirstOrDefault();
            if (todayForecast != null)
            {
                weatherData.Sunrise = todayForecast.Astro?.Sunrise;
                weatherData.Sunset = todayForecast.Astro?.Sunset;

                var maxRainChance = todayForecast.Hour?.Max(h => h.ChanceOfRain) ?? 0;
                weatherData.ChanceOfRainToday = maxRainChance;
                weatherData.WillItRainToday = maxRainChance >= _rainThresholdPercent;
            }

            return weatherData;
        }

        public List<ForecastDay> MapToForecastDays(ForecastResponseDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var forecastDays = new List<ForecastDay>();

            if (dto.Forecast?.Forecastday == null)
                return forecastDays;

            foreach (var dayDto in dto.Forecast.Forecastday)
            {
                var forecastDay = new ForecastDay
                {
                    Date = DateTime.TryParse(dayDto.Date, out var date) ? date : DateTime.UtcNow,

                    MaxTempC = dayDto.Day?.MaxtempC ?? 0,
                    MinTempC = dayDto.Day?.MintempC ?? 0,
                    AvgTempC = dayDto.Day?.AvgtempC ?? 0,
                    MaxTempF = dayDto.Day?.MaxtempF ?? 0,
                    MinTempF = dayDto.Day?.MintempF ?? 0,
                    AvgTempF = dayDto.Day?.AvgtempF ?? 0,

                    ConditionText = dayDto.Day?.Condition?.Text,
                    ConditionIcon = FixIconUrl(dayDto.Day?.Condition?.Icon),
                    ConditionCode = dayDto.Day?.Condition?.Code ?? 0,

                    MaxWindKph = dayDto.Day?.MaxwindKph ?? 0,
                    TotalPrecipMm = dayDto.Day?.TotalprecipMm ?? 0,
                    AvgHumidity = dayDto.Day?.Avghumidity ?? 0,
                    UVIndex = dayDto.Day?.Uv ?? 0,
                    AvgVisibilityKm = dayDto.Day?.AvgvisKm ?? 0,

                    Sunrise = dayDto.Astro?.Sunrise,
                    Sunset = dayDto.Astro?.Sunset,
                    Moonrise = dayDto.Astro?.Moonrise,
                    Moonset = dayDto.Astro?.Moonset,
                    MoonPhase = dayDto.Astro?.MoonPhase,
                    MoonIllumination = dayDto.Astro?.MoonIllumination ?? 0
                };

                // Маппинг почасового прогноза
                if (dayDto.Hour != null)
                {
                    foreach (var hourDto in dayDto.Hour)
                    {
                        var hour = new HourlyForecast
                        {
                            Time = DateTime.TryParse(hourDto.Time, out var time) ? time : DateTime.UtcNow,

                            TemperatureC = hourDto.TempC,
                            TemperatureF = hourDto.TempF,
                            FeelsLikeC = hourDto.FeelslikeC,
                            FeelsLikeF = hourDto.FeelslikeF,

                            ConditionText = hourDto.Condition?.Text,
                            ConditionIcon = FixIconUrl(hourDto.Condition?.Icon),
                            ConditionCode = hourDto.Condition?.Code ?? 0,
                            IsDay = hourDto.IsDay == 1,

                            Humidity = hourDto.Humidity,
                            WindSpeedKph = hourDto.WindKph,
                            WindSpeedMph = hourDto.WindMph,
                            PressureMb = hourDto.PressureMb,
                            PrecipitationMm = hourDto.PrecipMm,
                            CloudCover = hourDto.Cloud,
                            VisibilityKm = hourDto.VisKm,

                            ChanceOfRain = hourDto.ChanceOfRain,
                            ChanceOfSnow = hourDto.ChanceOfSnow,
                            WillItRain = hourDto.WillItRain == 1,
                            WillItSnow = hourDto.WillItSnow == 1
                        };

                        forecastDay.Hours.Add(hour);
                    }
                }

                forecastDays.Add(forecastDay);
            }

            return forecastDays;
        }

        private string? FixIconUrl(string? icon)
        {
            if (string.IsNullOrEmpty(icon))
                return null;

            if (icon.StartsWith("//"))
                return "https:" + icon;

            return icon;
        }

        private WeatherData CreateWeatherData(LocationDto? location, CurrentWeatherDto? current)
        {
            return new WeatherData
            {
                CityName = location?.Name,
                Country = location?.Country,
                Region = location?.Region,
                Latitude = location?.Lat ?? 0,
                Longitude = location?.Lon ?? 0,

                TemperatureC = current?.TempC ?? 0,
                TemperatureF = current?.TempF ?? 0,
                FeelsLikeC = current?.FeelslikeC ?? 0,
                FeelsLikeF = current?.FeelslikeF ?? 0,

                ConditionText = current?.Condition?.Text,
                ConditionIcon = FixIconUrl(current?.Condition?.Icon),
                ConditionCode = current?.Condition?.Code ?? 0,
                IsDay = current?.IsDay == 1,

                Humidity = current?.Humidity ?? 0,
                WindSpeedKph = current?.WindKph ?? 0,
                WindSpeedMph = current?.WindMph ?? 0,
                WindDirection = current?.WindDir,
                PressureMb = current?.PressureMb ?? 0,
                PressureIn = current?.PressureIn ?? 0,
                PrecipitationMm = current?.PrecipMm ?? 0,
                PrecipitationIn = current?.PrecipIn ?? 0,
                UVIndex = current?.Uv ?? 0,
                VisibilityKm = current?.VisKm ?? 0,
                VisibilityMiles = current?.VisMiles ?? 0,
                CloudCover = current?.Cloud ?? 0,

                AirQuality = MapAirQuality(current?.AirQuality),

                LastUpdated = DateTime.TryParse(current?.LastUpdated, out var lastUpdated)
                    ? lastUpdated
                    : DateTime.UtcNow,

                IsCached = false
            };
        }

        private AirQualityData? MapAirQuality(AirQualityDto? airQualityDto)
        {
            if (airQualityDto == null)
                return null;

            return new AirQualityData
            {
                Co = airQualityDto.Co,
                No2 = airQualityDto.No2,
                O3 = airQualityDto.O3,
                So2 = airQualityDto.So2,
                Pm25 = airQualityDto.Pm25,
                Pm10 = airQualityDto.Pm10,
                UsEpaIndex = airQualityDto.UsEpaIndex,
                GbDefraIndex = airQualityDto.GbDefraIndex
            };
        }
    }
}