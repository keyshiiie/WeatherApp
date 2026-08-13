using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using WeatherApp.Core.Configuration;
using WeatherApp.Core.DTOs;
using WeatherApp.Core.Services;
using Xunit;

namespace WeatherApp.Tests.Services
{
    public class WeatherServiceTests
    {
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<ILogger<WeatherService>> _loggerMock;
        private readonly WeatherService _weatherService;

        public WeatherServiceTests()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

            // Создаем HttpClient с мок-обработчиком
            var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://api.weatherapi.com/v1")
            };

            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _httpClientFactoryMock
                .Setup(x => x.CreateClient("WeatherApi"))
                .Returns(httpClient);

            _loggerMock = new Mock<ILogger<WeatherService>>();


            //var optionsMock = new Mock<IOptions<>>();
            //optionsMock.Setup(x => x.Value).Returns();

            _weatherService = new WeatherService(
                _httpClientFactoryMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task GetCurrentWeatherAsync_WithValidCity_ReturnsWeatherData()
        {
            // Arrange
            var cityName = "London";
            var expectedResponse = CreateSampleWeatherResponse();

            var responseJson = JsonSerializer.Serialize(expectedResponse);
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            };

            SetupHttpClientResponse(responseMessage);

            // Act
            var result = await _weatherService.GetCurrentWeatherAsync(cityName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("London", result.CityName);
            Assert.Equal("United Kingdom", result.Country);
            Assert.Equal(15.5, result.TemperatureC);
            Assert.Equal("Partly cloudy", result.ConditionText);
            Assert.Equal(72, result.Humidity);
        }

        [Fact]
        public async Task GetCurrentWeatherAsync_WithEmptyCity_ReturnsNull()
        {
            // Act
            var result = await _weatherService.GetCurrentWeatherAsync("");

            // Assert
            Assert.Null(result);
            VerifyLog(LogLevel.Warning, "City name is empty", Times.Once);
        }

        [Fact]
        public async Task GetCurrentWeatherAsync_WhenApiReturnsError_ReturnsNull()
        {
            // Arrange
            var cityName = "InvalidCity";
            var responseMessage = new HttpResponseMessage(HttpStatusCode.NotFound);

            SetupHttpClientResponse(responseMessage);

            // Act
            var result = await _weatherService.GetCurrentWeatherAsync(cityName);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetCurrentWeatherAsync_WithValidCoordinates_ReturnsWeatherData()
        {
            // Arrange
            var latitude = 51.5074;
            var longitude = -0.1278;
            var expectedResponse = CreateSampleWeatherResponse();

            var responseJson = JsonSerializer.Serialize(expectedResponse);
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            };

            SetupHttpClientResponse(responseMessage);

            // Act
            var result = await _weatherService.GetCurrentWeatherAsync(latitude, longitude);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("London", result.CityName);
            Assert.Equal(15.5, result.TemperatureC);
        }

        [Fact]
        public async Task GetForecastAsync_WithValidCity_ReturnsForecastDays()
        {
            // Arrange
            var cityName = "London";
            var days = 5;
            var expectedResponse = CreateSampleForecastResponse();

            var responseJson = JsonSerializer.Serialize(expectedResponse);
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            };

            SetupHttpClientResponse(responseMessage);

            // Act
            var result = await _weatherService.GetForecastAsync(cityName, days);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count); // У нас 2 дня в sample
            Assert.Equal("Sunny", result[0].ConditionText);
            Assert.Equal(25.0, result[0].MaxTempC);
            Assert.Equal(18.0, result[0].MinTempC);
        }

        [Fact]
        public async Task SearchCitiesAsync_WithValidQuery_ReturnsCitySuggestions()
        {
            // Arrange
            var query = "Lon";
            var expectedResponse = new[]
            {
                new SearchResponseDto
                {
                    Id = 1,
                    Name = "London",
                    Region = "Greater London",
                    Country = "United Kingdom",
                    Lat = 51.5074,
                    Lon = -0.1278
                },
                new SearchResponseDto
                {
                    Id = 2,
                    Name = "Los Angeles",
                    Region = "California",
                    Country = "United States",
                    Lat = 34.0522,
                    Lon = -118.2437
                }
            };

            var responseJson = JsonSerializer.Serialize(expectedResponse);
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            };

            SetupHttpClientResponse(responseMessage);

            // Act
            var result = await _weatherService.SearchCitiesAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("London", result[0].Name);
            Assert.Equal("Los Angeles", result[1].Name);
        }

        [Fact]
        public async Task SearchCitiesAsync_WithShortQuery_ReturnsEmptyList()
        {
            // Act
            var result = await _weatherService.SearchCitiesAsync("L");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetCurrentAndForecastAsync_WithValidCity_ReturnsBoth()
        {
            // Arrange
            var cityName = "London";
            var days = 5;
            var expectedResponse = CreateSampleForecastResponse();

            var responseJson = JsonSerializer.Serialize(expectedResponse);
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            };

            SetupHttpClientResponse(responseMessage);

            // Act
            var (current, forecast) = await _weatherService.GetCurrentAndForecastAsync(cityName, days);

            // Assert
            Assert.NotNull(current);
            Assert.NotNull(forecast);
            Assert.Equal("London", current.CityName);
            Assert.Equal(15.5, current.TemperatureC);
            Assert.Equal(2, forecast.Count);
        }

        [Fact]
        public async Task GetCurrentWeatherAsync_WithValidCity_CancellationToken_Works()
        {
            // Arrange
            var cityName = "London";
            var cts = new CancellationTokenSource();
            var expectedResponse = CreateSampleWeatherResponse();

            var responseJson = JsonSerializer.Serialize(expectedResponse);
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            };

            SetupHttpClientResponse(responseMessage);

            // Act
            var result = await _weatherService.GetCurrentWeatherAsync(cityName, cts.Token);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("London", result.CityName);
        }

        [Fact]
        public async Task GetCurrentWeatherAsync_WithValidCoordinates_WhenApiReturnsNoCurrent_ReturnsNull()
        {
            // Arrange
            var latitude = 51.5074;
            var longitude = -0.1278;
            var response = new WeatherResponseDto
            {
                Location = new LocationDto { Name = "London" },
                Current = null // No current data
            };

            var responseJson = JsonSerializer.Serialize(response);
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            };

            SetupHttpClientResponse(responseMessage);

            // Act
            var result = await _weatherService.GetCurrentWeatherAsync(latitude, longitude);

            // Assert
            Assert.Null(result);
            VerifyLog(LogLevel.Warning, "No weather data received for coordinates", Times.Once);
        }

        [Fact]
        public async Task GetCurrentWeatherAsync_WithValidCity_WhenApiReturnsNoCurrent_ReturnsNull()
        {
            // Arrange
            var cityName = "London";
            var response = new WeatherResponseDto
            {
                Location = new LocationDto { Name = "London" },
                Current = null // No current data
            };

            var responseJson = JsonSerializer.Serialize(response);
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            };

            SetupHttpClientResponse(responseMessage);

            // Act
            var result = await _weatherService.GetCurrentWeatherAsync(cityName);

            // Assert
            Assert.Null(result);
            VerifyLog(LogLevel.Warning, "No weather data received for city", Times.Once);
        }

        [Fact]
        public async Task GetForecastAsync_WithValidCity_WithDefaultDays_ReturnsForecast()
        {
            // Arrange
            var cityName = "London";
            var expectedResponse = CreateSampleForecastResponse();

            var responseJson = JsonSerializer.Serialize(expectedResponse);
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            };

            SetupHttpClientResponse(responseMessage);

            // Act
            var result = await _weatherService.GetForecastAsync(cityName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetForecastAsync_WithValidCity_WhenApiReturnsEmptyForecast_ReturnsNull()
        {
            // Arrange
            var cityName = "London";
            var response = new ForecastResponseDto
            {
                Location = new LocationDto { Name = "London" },
                Forecast = new ForecastDto { Forecastday = new List<ForecastDayDto>() } // Empty forecast
            };

            var responseJson = JsonSerializer.Serialize(response);
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            };

            SetupHttpClientResponse(responseMessage);

            // Act
            var result = await _weatherService.GetForecastAsync(cityName);

            // Assert
            Assert.Null(result);
            VerifyLog(LogLevel.Warning, "No forecast data received for city", Times.Once);
        }

        [Fact]
        public async Task GetForecastAsync_WithValidCoordinates_WhenApiReturnsEmptyForecast_ReturnsNull()
        {
            // Arrange
            var latitude = 51.5074;
            var longitude = -0.1278;
            var response = new ForecastResponseDto
            {
                Location = new LocationDto { Name = "London" },
                Forecast = new ForecastDto { Forecastday = new List<ForecastDayDto>() } // Empty forecast
            };

            var responseJson = JsonSerializer.Serialize(response);
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            };

            SetupHttpClientResponse(responseMessage);

            // Act
            var result = await _weatherService.GetForecastAsync(latitude, longitude);

            // Assert
            Assert.Null(result);
            VerifyLog(LogLevel.Warning, "No forecast data received for coordinates", Times.Once);
        }

        [Fact]
        public async Task GetForecastAsync_WithValidCity_WhenApiReturnsNullForecast_ReturnsNull()
        {
            // Arrange
            var cityName = "London";
            var response = new ForecastResponseDto
            {
                Location = new LocationDto { Name = "London" },
                Forecast = null // Null forecast
            };

            var responseJson = JsonSerializer.Serialize(response);
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            };

            SetupHttpClientResponse(responseMessage);

            // Act
            var result = await _weatherService.GetForecastAsync(cityName);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetForecastAsync_WithEmptyCity_ReturnsNull()
        {
            // Act
            var result = await _weatherService.GetForecastAsync("");

            // Assert
            Assert.Null(result);
            VerifyLog(LogLevel.Warning, "City name is empty", Times.Once);
        }

        [Fact]
        public async Task GetForecastAsync_WithDaysLessThan1_ClampsTo1()
        {
            // Arrange
            var cityName = "London";
            var days = 0;
            var expectedResponse = CreateSampleForecastResponse();

            var responseJson = JsonSerializer.Serialize(expectedResponse);
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            };

            SetupHttpClientResponse(responseMessage);

            // Act
            var result = await _weatherService.GetForecastAsync(cityName, days);

            // Assert
            Assert.NotNull(result);
            // Проверяем, что запрос был отправлен с days=1 (минимальное значение)
            _httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri!.ToString().Contains("days=1")),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task GetForecastAsync_WithDaysMoreThan14_ClampsTo14()
        {
            // Arrange
            var cityName = "London";
            var days = 20;
            var expectedResponse = CreateSampleForecastResponse();

            var responseJson = JsonSerializer.Serialize(expectedResponse);
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            };

            SetupHttpClientResponse(responseMessage);

            // Act
            var result = await _weatherService.GetForecastAsync(cityName, days);

            // Assert
            Assert.NotNull(result);
            // Проверяем, что запрос был отправлен с days=14 (максимальное значение)
            _httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri!.ToString().Contains("days=14")),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task SearchCitiesAsync_WhenApiReturnsNull_ReturnsEmptyList()
        {
            // Arrange
            var query = "London";
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("null", Encoding.UTF8, "application/json")
            };

            SetupHttpClientResponse(responseMessage);

            // Act
            var result = await _weatherService.SearchCitiesAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task SearchCitiesAsync_WhenApiReturnsEmptyList_ReturnsEmptyList()
        {
            // Arrange
            var query = "London";
            var responseJson = JsonSerializer.Serialize(new List<SearchResponseDto>());
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            };

            SetupHttpClientResponse(responseMessage);

            // Act
            var result = await _weatherService.SearchCitiesAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task SearchCitiesAsync_WithNullQuery_ReturnsEmptyList()
        {
            // Act
            var result = await _weatherService.SearchCitiesAsync(null!);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetCurrentAndForecastAsync_WithValidCity_WhenCurrentIsNull_ReturnsNullCurrent()
        {
            // Arrange
            var cityName = "London";
            var days = 5;
            var response = new ForecastResponseDto
            {
                Location = new LocationDto { Name = "London" },
                Current = null, // No current data
                Forecast = new ForecastDto
                {
                    Forecastday = new List<ForecastDayDto>
            {
                new ForecastDayDto
                {
                    Date = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd"),
                    Day = new DayDataDto
                    {
                        MaxtempC = 25.0,
                        MintempC = 18.0,
                        Condition = new ConditionDto { Text = "Sunny" }
                    }
                }
            }
                }
            };

            var responseJson = JsonSerializer.Serialize(response);
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            };

            SetupHttpClientResponse(responseMessage);

            // Act
            var (current, forecast) = await _weatherService.GetCurrentAndForecastAsync(cityName, days);

            // Assert
            Assert.Null(current);
            Assert.NotNull(forecast);
            Assert.Single(forecast);
        }

        [Fact]
        public async Task GetCurrentAndForecastAsync_WithValidCity_WhenForecastIsNull_ReturnsNullForecast()
        {
            // Arrange
            var cityName = "London";
            var days = 5;
            var response = new ForecastResponseDto
            {
                Location = new LocationDto { Name = "London" },
                Current = new CurrentWeatherDto
                {
                    TempC = 15.5,
                    Condition = new ConditionDto { Text = "Partly cloudy" }
                },
                Forecast = null // No forecast data
            };

            var responseJson = JsonSerializer.Serialize(response);
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            };

            SetupHttpClientResponse(responseMessage);

            // Act
            var (current, forecast) = await _weatherService.GetCurrentAndForecastAsync(cityName, days);

            // Assert
            Assert.NotNull(current);
            Assert.Null(forecast);
        }

        [Fact]
        public async Task GetCurrentAndForecastAsync_WithEmptyCity_ReturnsNullBoth()
        {
            // Act
            var (current, forecast) = await _weatherService.GetCurrentAndForecastAsync("");

            // Assert
            Assert.Null(current);
            Assert.Null(forecast);
            VerifyLog(LogLevel.Warning, "City name is empty", Times.Once);
        }

        [Fact]
        public async Task GetCurrentAndForecastAsync_WithValidCoordinates_WhenApiReturnsNull_ReturnsNullBoth()
        {
            // Arrange
            var latitude = 51.5074;
            var longitude = -0.1278;
            var responseMessage = new HttpResponseMessage(HttpStatusCode.NotFound);

            SetupHttpClientResponse(responseMessage);

            // Act
            var (current, forecast) = await _weatherService.GetCurrentAndForecastAsync(latitude, longitude);

            // Assert
            Assert.Null(current);
            Assert.Null(forecast);
        }

        [Fact]
        public async Task GetCurrentAndForecastAsync_WithValidCity_WithDaysMoreThan14_ClampsTo14()
        {
            // Arrange
            var cityName = "London";
            var days = 20;
            var expectedResponse = CreateSampleForecastResponse();

            var responseJson = JsonSerializer.Serialize(expectedResponse);
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            };

            SetupHttpClientResponse(responseMessage);

            // Act
            var (current, forecast) = await _weatherService.GetCurrentAndForecastAsync(cityName, days);

            // Assert
            Assert.NotNull(current);
            Assert.NotNull(forecast);
            // Проверяем, что запрос был отправлен с days=14
            _httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri!.ToString().Contains("days=14")),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task GetCurrentAndForecastAsync_WithValidCoordinates_AndEmptyForecast_ReturnsNullForecast()
        {
            // Arrange
            var latitude = 51.5074;
            var longitude = -0.1278;
            var response = new ForecastResponseDto
            {
                Location = new LocationDto { Name = "London" },
                Current = new CurrentWeatherDto
                {
                    TempC = 15.5,
                    Condition = new ConditionDto { Text = "Partly cloudy" }
                },
                Forecast = new ForecastDto { Forecastday = new List<ForecastDayDto>() } // Empty forecast
            };

            var responseJson = JsonSerializer.Serialize(response);
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            };

            SetupHttpClientResponse(responseMessage);

            // Act
            var (current, forecast) = await _weatherService.GetCurrentAndForecastAsync(latitude, longitude);

            // Assert
            Assert.NotNull(current);
            Assert.Null(forecast);
        }

        // Тест для проверки обработки HttpRequestException в GetCurrentAndForecastAsync
        [Fact]
        public async Task GetCurrentAndForecastAsync_WithHttpRequestException_ReturnsNullBoth()
        {
            // Arrange
            var cityName = "London";
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Act
            var (current, forecast) = await _weatherService.GetCurrentAndForecastAsync(cityName);

            // Assert
            Assert.Null(current);
            Assert.Null(forecast);
            VerifyLog(LogLevel.Error, "HTTP error while fetching weather and forecast", Times.Once);
        }

        // Тест для проверки обработки HttpRequestException в SearchCitiesAsync
        [Fact]
        public async Task SearchCitiesAsync_WithHttpRequestException_ReturnsNull()
        {
            // Arrange
            var query = "London";
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Act
            var result = await _weatherService.SearchCitiesAsync(query);

            // Assert
            Assert.Null(result);
            VerifyLog(LogLevel.Error, "HTTP error while searching cities", Times.Once);
        }

        // Тест для проверки обработки общего исключения в SearchCitiesAsync
        [Fact]
        public async Task SearchCitiesAsync_WithGeneralException_ReturnsNull()
        {
            // Arrange
            var query = "London";
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new InvalidOperationException("Something went wrong"));

            // Act
            var result = await _weatherService.SearchCitiesAsync(query);

            // Assert
            Assert.Null(result);
            VerifyLog(LogLevel.Error, "Unexpected error while searching cities", Times.Once);
        }

        // Тест для проверки CancellationToken в GetForecastAsync
        [Fact]
        public async Task GetForecastAsync_WithCancellationToken_Works()
        {
            // Arrange
            var cityName = "London";
            var cts = new CancellationTokenSource();
            var expectedResponse = CreateSampleForecastResponse();

            var responseJson = JsonSerializer.Serialize(expectedResponse);
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            };

            SetupHttpClientResponse(responseMessage);

            // Act
            var result = await _weatherService.GetForecastAsync(cityName, 5, cts.Token);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        // Тест для проверки cancellation token в SearchCitiesAsync
        [Fact]
        public async Task SearchCitiesAsync_WithCancellationToken_Works()
        {
            // Arrange
            var query = "London";
            var cts = new CancellationTokenSource();
            var expectedResponse = new[]
            {
                new SearchResponseDto
                {
                    Id = 1,
                    Name = "London",
                    Region = "Greater London",
                    Country = "United Kingdom",
                    Lat = 51.5074,
                    Lon = -0.1278
                }
            };

            var responseJson = JsonSerializer.Serialize(expectedResponse);
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            };

            SetupHttpClientResponse(responseMessage);

            // Act
            var result = await _weatherService.SearchCitiesAsync(query, cts.Token);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }


        #region Helper Methods

        private void VerifyLog(LogLevel level, string expectedSubstring, Func<Times> times)
        {
            _loggerMock.Verify(
                x => x.Log(
                    level,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) =>
                        v != null &&
                        v.ToString() != null &&
                        v.ToString()!.Contains(expectedSubstring)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                times());
        }

        private void SetupHttpClientResponse(HttpResponseMessage responseMessage)
        {
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);
        }

        private WeatherResponseDto CreateSampleWeatherResponse()
        {
            return new WeatherResponseDto
            {
                Location = new LocationDto
                {
                    Name = "London",
                    Country = "United Kingdom",
                    Lat = 51.5074,
                    Lon = -0.1278
                },
                Current = new CurrentWeatherDto
                {
                    TempC = 15.5,
                    TempF = 59.9,
                    Condition = new ConditionDto
                    {
                        Text = "Partly cloudy",
                        Code = 1003,
                        Icon = "//cdn.weatherapi.com/weather/64x64/day/116.png"
                    },
                    Humidity = 72,
                    WindKph = 12.6,
                    PressureMb = 1012,
                    PrecipMm = 0.0,
                    Uv = 3,
                    VisKm = 10.0,
                    Cloud = 50,
                    IsDay = 1,
                    LastUpdated = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm")
                }
            };
        }

        private ForecastResponseDto CreateSampleForecastResponse()
        {
            return new ForecastResponseDto
            {
                Location = new LocationDto
                {
                    Name = "London",
                    Country = "United Kingdom",
                    Lat = 51.5074,
                    Lon = -0.1278
                },
                Current = new CurrentWeatherDto
                {
                    TempC = 15.5,
                    TempF = 59.9,
                    Condition = new ConditionDto
                    {
                        Text = "Partly cloudy",
                        Code = 1003,
                        Icon = "//cdn.weatherapi.com/weather/64x64/day/116.png"
                    },
                    Humidity = 72,
                    WindKph = 12.6,
                    PressureMb = 1012,
                    PrecipMm = 0.0,
                    Uv = 3,
                    VisKm = 10.0,
                    Cloud = 50,
                    IsDay = 1,
                    LastUpdated = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm")
                },
                Forecast = new ForecastDto
                {
                    Forecastday = new List<ForecastDayDto>
                    {
                        new ForecastDayDto
                        {
                            Date = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd"),
                            Day = new DayDataDto
                            {
                                MaxtempC = 25.0,
                                MintempC = 18.0,
                                AvgtempC = 21.5,
                                Condition = new ConditionDto
                                {
                                    Text = "Sunny",
                                    Code = 1000,
                                    Icon = "//cdn.weatherapi.com/weather/64x64/day/113.png"
                                },
                                Avghumidity = 65,
                                TotalprecipMm = 0.0,
                                MaxwindKph = 15.0,
                                Uv = 5
                            },
                            Astro = new AstroDto
                            {
                                Sunrise = "06:30 AM",
                                Sunset = "08:15 PM"
                            },
                            Hour = new List<HourlyForecastDto>
                            {
                                new HourlyForecastDto
                                {
                                    Time = DateTime.UtcNow.AddDays(1).AddHours(12).ToString("yyyy-MM-dd HH:mm"),
                                    TempC = 22.0,
                                    Condition = new ConditionDto
                                    {
                                        Text = "Sunny",
                                        Code = 1000,
                                        Icon = "//cdn.weatherapi.com/weather/64x64/day/113.png"
                                    },
                                    Humidity = 60,
                                    WindKph = 12.0,
                                    PressureMb = 1010,
                                    PrecipMm = 0.0,
                                    Cloud = 10,
                                    IsDay = 1,
                                    ChanceOfRain = 0,
                                    ChanceOfSnow = 0
                                }
                            }
                        },
                        new ForecastDayDto
                        {
                            Date = DateTime.UtcNow.AddDays(2).ToString("yyyy-MM-dd"),
                            Day = new DayDataDto
                            {
                                MaxtempC = 20.0,
                                MintempC = 15.0,
                                AvgtempC = 17.5,
                                Condition = new ConditionDto
                                {
                                    Text = "Cloudy",
                                    Code = 1006,
                                    Icon = "//cdn.weatherapi.com/weather/64x64/day/119.png"
                                },
                                Avghumidity = 75,
                                TotalprecipMm = 2.0,
                                MaxwindKph = 20.0,
                                Uv = 3
                            },
                            Astro = new AstroDto
                            {
                                Sunrise = "06:31 AM",
                                Sunset = "08:14 PM"
                            }
                        }
                    }
                }
            };
        }

        #endregion
    }
}