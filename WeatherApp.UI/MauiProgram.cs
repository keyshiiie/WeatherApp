using CommunityToolkit.Maui;
using Microcharts.Maui;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WeatherApp.Core.Constants;
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
            .UseMicrocharts()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("Inter-Light.otf", "InterLight");
                fonts.AddFont("Inter-Regular.otf", "InterRegular");
                fonts.AddFont("Inter-Medium.otf", "InterMedium");
                fonts.AddFont("Inter-SemiBold.otf", "InterSemiBold");
                fonts.AddFont("Inter-Bold.otf", "InterBold");
                fonts.AddFont("Inter-ExtraBold.otf", "InterExtraBold");
            });

        // Настройка логирования для MAUI
#if DEBUG
        builder.Logging.AddDebug();
        builder.Logging.SetMinimumLevel(LogLevel.Debug);
#else
        builder.Logging.SetMinimumLevel(LogLevel.Warning);
#endif

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "weatherapp.db");
        var connectionString = $"Data Source={dbPath}";

        System.Diagnostics.Debug.WriteLine($"Database path: {dbPath}");

        var directory = Path.GetDirectoryName(dbPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        builder.Services.AddHttpClient("WeatherApi", client =>
        {
            client.BaseAddress = new Uri(ApiConstants.WeatherApiBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        builder.Services.AddHttpClient<GeolocationService>(client =>
        {
            client.BaseAddress = new Uri(ApiConstants.NominatimBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        RegisterServices(builder.Services);
        RegisterRepositories(builder.Services);
        RegisterViewModels(builder.Services);
        RegisterPages(builder.Services);

        var app = builder.Build();
        InitializeDatabase(app.Services);

        return app;
    }

    private static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IWeatherService, WeatherService>();
        services.AddSingleton<IGeolocationService, GeolocationService>();
        services.AddScoped<IWeatherRepository, WeatherRepository>();
        services.AddScoped<IFavoritesRepository, FavoritesRepository>();
        services.AddScoped<IHistoryRepository, HistoryRepository>();
        services.AddScoped<ICityService, CityService>();
        services.AddSingleton<INotificationService, NotificationService>();
        services.AddSingleton<WeatherAlertService>();
        services.AddSingleton<ISettingsService, SettingsService>();
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
        services.AddTransient<LoginPageViewModel>();
    }

    private static void RegisterPages(IServiceCollection services)
    {
        services.AddTransient<MainPage>();
        services.AddTransient<CurrentWeatherPage>();
        services.AddTransient<ForecastPage>();
        services.AddTransient<DetailsPage>();
        services.AddTransient<FavoritesPage>();
        services.AddTransient<SettingsPage>();
        services.AddTransient<LoginPage>();
    }

    private static void InitializeDatabase(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        try
        {
            logger.LogInformation("Initializing database...");
            var created = dbContext.Database.EnsureCreated();

            if (created)
            {
                logger.LogInformation("✅ Database created successfully");
            }
            else
            {
                logger.LogInformation("✅ Database already exists");
            }

            logger.LogInformation("📁 Database location: {DbPath}",
                Path.Combine(FileSystem.AppDataDirectory, "weatherapp.db"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Database initialization error");
        }
    }
}