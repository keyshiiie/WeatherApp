using CommunityToolkit.Maui;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WeatherApp.Core.Configuration;
using WeatherApp.Core.Data;
using WeatherApp.Core.Repositories;
using WeatherApp.Core.Services;
using WeatherApp.Core.ViewModels;
using WeatherApp.UI.Services;
using WeatherApp.UI.ViewModels;
using WeatherApp.UI.Views;

namespace WeatherApp.UI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // 1. Настройка Configuration с поддержкой переменных окружения
        var basePath = AppContext.BaseDirectory;

        var config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            // Базовый файл с общими настройками
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            // Файл для разработки (не в Git)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
            .Build();

        builder.Configuration.AddConfiguration(config);

        // Для отладки - проверяем, загрузился ли ключ
        var apiKey = config.GetSection("ApiSettings")["WeatherApiKey"];
        System.Diagnostics.Debug.WriteLine($"🔑 API Key загружен: {(string.IsNullOrEmpty(apiKey) ? "❌ НЕТ" : "✅ ДА")}");
        System.Diagnostics.Debug.WriteLine($"📁 BasePath: {basePath}");

        // 2. Регистрация настроек через IOptions
        builder.Services.Configure<ApiSettings>(
            builder.Configuration.GetSection("ApiSettings"));

        builder.Services.Configure<CacheSettings>(
            builder.Configuration.GetSection("CacheSettings"));

        // 3. Регистрация DbContext
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                               ?? "Data Source=weatherapp.db";

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        // 4. Регистрация HttpClient
        builder.Services.AddHttpClient("WeatherApi", (sp, client) =>
        {
            var apiSettings = sp.GetRequiredService<IOptions<ApiSettings>>().Value;

            // Проверяем, что ключ загружен
            if (string.IsNullOrEmpty(apiSettings?.WeatherApiKey))
            {
                System.Diagnostics.Debug.WriteLine("⚠️ ВНИМАНИЕ: WeatherApiKey не загружен!");
            }

            var baseUrl = apiSettings?.WeatherApiBaseUrl ?? "https://api.weatherapi.com/v1";
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            System.Diagnostics.Debug.WriteLine($"✅ HttpClient настроен на: {baseUrl}");
        });

        // 5. Регистрация сервисов
        RegisterServices(builder.Services);

        // 6. Регистрация репозиториев
        RegisterRepositories(builder.Services);

        // 7. Регистрация ViewModels
        RegisterViewModels(builder.Services);

        // 8. Регистрация страниц
        RegisterPages(builder.Services);

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        // Применение миграций БД
        ApplyMigrations(app.Services);

        return app;
    }

    private static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IWeatherService, WeatherService>();
        services.AddSingleton<IGeolocationService, GeolocationService>();
        services.AddSingleton<IFavoritesService, FavoritesService>();
        services.AddSingleton<INotificationService, NotificationService>();
        services.AddSingleton<WeatherAlertService>();
    }

    private static void RegisterRepositories(IServiceCollection services)
    {
        services.AddScoped<IWeatherRepository, WeatherRepository>();
    }

    private static void RegisterViewModels(IServiceCollection services)
    {
        services.AddTransient<MainPageViewModel>();
        services.AddTransient<CurrentWeatherViewModel>();
        services.AddTransient<ForecastPageViewModel>();
        services.AddTransient<DetailsPageViewModel>();
        services.AddTransient<FavoritesPageViewModel>();
        services.AddTransient<SettingsPageViewModel>();
    }

    private static void RegisterPages(IServiceCollection services)
    {
        services.AddTransient<MainPage>();
        services.AddTransient<CurrentWeatherPage>();
        services.AddTransient<ForecastPage>();
        services.AddTransient<DetailsPage>();
        services.AddTransient<FavoritesPage>();
        services.AddTransient<SettingsPage>();
    }

    private static void ApplyMigrations(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            dbContext.Database.Migrate();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка миграции БД: {ex.Message}");
        }
    }
}