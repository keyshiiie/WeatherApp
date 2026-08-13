using CommunityToolkit.Maui;
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
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        var connectionString = "Data Source=weatherapp.db";
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

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();
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