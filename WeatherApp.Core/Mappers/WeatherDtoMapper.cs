using System;
using System.Collections.Generic;
using System.Text;
using WeatherApp.Core.DTOs;
using WeatherApp.Core.Models;

namespace WeatherApp.Core.Mappers
{
    public class WeatherDtoMapper
    {
        public static WeatherData MapToWeatherData(WeatherResponseDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            return new WeatherData
            {
                CityName = dto.Location?.Name,
                Country = dto.Location?.Country,
                Region = dto.Location?.Region,
                Latitude = dto.Location?.Lat ?? 0,
                Longitude = dto.Location?.Lon ?? 0,

                TemperatureC = dto.Current?.TempC ?? 0,
                TemperatureF = dto.Current?.TempF ?? 0,
                FeelsLikeC = dto.Current?.FeelslikeC ?? 0,
                FeelsLikeF = dto.Current?.FeelslikeF ?? 0,

                ConditionText = dto.Current?.Condition?.Text,
                ConditionIcon = dto.Current?.Condition?.Icon,
                ConditionCode = dto.Current?.Condition?.Code ?? 0,
                IsDay = dto.Current?.IsDay == 1,

                Humidity = dto.Current?.Humidity ?? 0,
                WindSpeedKph = dto.Current?.WindKph ?? 0,
                WindSpeedMph = dto.Current?.WindMph ?? 0,
                WindDirection = dto.Current?.WindDir,
                PressureMb = dto.Current?.PressureMb ?? 0,
                PressureIn = dto.Current?.PressureIn ?? 0,
                PrecipitationMm = dto.Current?.PrecipMm ?? 0,
                PrecipitationIn = dto.Current?.PrecipIn ?? 0,
                UVIndex = dto.Current?.Uv ?? 0,
                VisibilityKm = dto.Current?.VisKm ?? 0,
                VisibilityMiles = dto.Current?.VisMiles ?? 0,
                CloudCover = dto.Current?.Cloud ?? 0,

                AirQuality = dto.Current?.AirQuality != null
                    ? new AirQualityData
                    {
                        Co = dto.Current.AirQuality.Co,
                        No2 = dto.Current.AirQuality.No2,
                        O3 = dto.Current.AirQuality.O3,
                        So2 = dto.Current.AirQuality.So2,
                        Pm25 = dto.Current.AirQuality.Pm25,
                        Pm10 = dto.Current.AirQuality.Pm10,
                        UsEpaIndex = dto.Current.AirQuality.UsEpaIndex,
                        GbDefraIndex = dto.Current.AirQuality.GbDefraIndex
                    }
                    : null,

                LastUpdated = DateTime.TryParse(dto.Current?.LastUpdated, out var lastUpdated)
                    ? lastUpdated
                    : DateTime.UtcNow,

                IsCached = false
            };
        }

        public static List<ForecastDay> MapToForecastDays(ForecastResponseDto dto)
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
                    ConditionIcon = dayDto.Day?.Condition?.Icon,
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
                            ConditionIcon = hourDto.Condition?.Icon,
                            ConditionCode = hourDto.Condition?.Code ?? 0,
                            IsDay = hourDto.IsDay == 1,

                            Humidity = hourDto.Humidity,
                            WindSpeedKph = hourDto.WindKph,
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

        /// <summary>
        /// Маппинг ForecastResponseDto → WeatherData (используя current)
        /// </summary>
        public static WeatherData MapToWeatherDataFromForecast(ForecastResponseDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            return new WeatherData
            {
                CityName = dto.Location?.Name,
                Country = dto.Location?.Country,
                Region = dto.Location?.Region,
                Latitude = dto.Location?.Lat ?? 0,
                Longitude = dto.Location?.Lon ?? 0,

                TemperatureC = dto.Current?.TempC ?? 0,
                TemperatureF = dto.Current?.TempF ?? 0,
                FeelsLikeC = dto.Current?.FeelslikeC ?? 0,
                FeelsLikeF = dto.Current?.FeelslikeF ?? 0,

                ConditionText = dto.Current?.Condition?.Text,
                ConditionIcon = dto.Current?.Condition?.Icon,
                ConditionCode = dto.Current?.Condition?.Code ?? 0,
                IsDay = dto.Current?.IsDay == 1,

                Humidity = dto.Current?.Humidity ?? 0,
                WindSpeedKph = dto.Current?.WindKph ?? 0,
                WindSpeedMph = dto.Current?.WindMph ?? 0,
                WindDirection = dto.Current?.WindDir,
                PressureMb = dto.Current?.PressureMb ?? 0,
                PressureIn = dto.Current?.PressureIn ?? 0,
                PrecipitationMm = dto.Current?.PrecipMm ?? 0,
                PrecipitationIn = dto.Current?.PrecipIn ?? 0,
                UVIndex = dto.Current?.Uv ?? 0,
                VisibilityKm = dto.Current?.VisKm ?? 0,
                VisibilityMiles = dto.Current?.VisMiles ?? 0,
                CloudCover = dto.Current?.Cloud ?? 0,

                AirQuality = dto.Current?.AirQuality != null
                    ? new AirQualityData
                    {
                        Co = dto.Current.AirQuality.Co,
                        No2 = dto.Current.AirQuality.No2,
                        O3 = dto.Current.AirQuality.O3,
                        So2 = dto.Current.AirQuality.So2,
                        Pm25 = dto.Current.AirQuality.Pm25,
                        Pm10 = dto.Current.AirQuality.Pm10,
                        UsEpaIndex = dto.Current.AirQuality.UsEpaIndex,
                        GbDefraIndex = dto.Current.AirQuality.GbDefraIndex
                    }
                    : null,

                LastUpdated = DateTime.TryParse(dto.Current?.LastUpdated, out var lastUpdated)
                    ? lastUpdated
                    : DateTime.UtcNow,

                IsCached = false
            };
        }

        public static CitySuggestion MapToCitySuggestion(SearchResponseDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            return new CitySuggestion
            {
                Id = dto.Id,
                Name = dto.Name,
                Region = dto.Region,
                Country = dto.Country,
                Latitude = dto.Lat,
                Longitude = dto.Lon,
                Url = dto.Url
            };
        }

        public static City MapToCity(GeocodingResponseDto dto)
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
}
