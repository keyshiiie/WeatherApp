using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WeatherApp.Core.Configuration;
using WeatherApp.Core.Data;
using WeatherApp.Core.Repositories;
using WeatherApp.Core.Services;
using WeatherApp.Core.ViewModels;
using WeatherApp.UI.Services;
using WeatherApp.UI.Views;

namespace WeatherApp.UI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // 1. Настройка Configuration
        var config = new ConfigurationBuilder()
        .SetBasePath(FileSystem.Current.AppDataDirectory)
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .Build();

        builder.Configuration.AddConfiguration(config);


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
        builder.Services.AddHttpClient();

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
        // Основные сервисы
        services.AddSingleton<IWeatherService, WeatherService>();
        services.AddSingleton<IGeolocationService, GeolocationService>();
        services.AddSingleton<IFavoritesService, FavoritesService>();

        // Сервисы уведомлений
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
        services.AddTransient<ForecastPageViewModel>();
        services.AddTransient<DetailsPageViewModel>();
        services.AddTransient<FavoritesPageViewModel>();
        services.AddTransient<SettingsPageViewModel>();
    }

    private static void RegisterPages(IServiceCollection services)
    {
        services.AddTransient<MainPage>();
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