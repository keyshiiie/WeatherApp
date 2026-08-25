using Microsoft.Extensions.Logging;
using WeatherApp.Core.Models;
using WeatherApp.Core.Services;
using WeatherApp.UI.Views;

namespace WeatherApp.UI.Services;

public class NavigationService : INavigationService
{
    private readonly ILogger<NavigationService> _logger;

    public NavigationService(ILogger<NavigationService> logger)
    {
        _logger = logger;
    }

    public async Task GoToAsync(string route)
    {
        try
        {
            await Shell.Current.GoToAsync(route);
            _logger.LogInformation($"Navigated to: {route}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Navigation failed to: {route}");
            throw;
        }
    }

    public async Task GoToAsync(string route, IDictionary<string, object> parameters)
    {
        try
        {
            await Shell.Current.GoToAsync(route, parameters);
            _logger.LogInformation($"Navigated to: {route} with {parameters.Count} parameters");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Navigation failed to: {route}");
            throw;
        }
    }

    public async Task GoBackAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("..");
            _logger.LogInformation("Navigated back");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Navigation back failed");
            throw;
        }
    }

    public async Task GoToMainPageAsync()
    {
        await GoToAsync("//main");
    }

    public async Task GoToWeatherPageAsync(City city)
    {
        var parameters = new Dictionary<string, object>
        {
            ["city"] = System.Text.Json.JsonSerializer.Serialize(city)
        };
        await GoToAsync(nameof(CurrentWeatherPage), parameters);
    }

    public async Task GoToWeatherPageAsync(int cityId)
    {
        var parameters = new Dictionary<string, object>
        {
            ["cityId"] = cityId
        };
        await GoToAsync(nameof(CurrentWeatherPage), parameters);
    }

    public async Task GoToFavoritesPageAsync()
    {
        await GoToAsync("//favorites");
    }

    public async Task GoToSettingsPageAsync()
    {
        await GoToAsync("//settings");
    }

    public async Task GoToLoginPageAsync()
    {
        await GoToAsync(nameof(LoginPage));
    }

    public async Task GoToChangeApiKeyPageAsync()
    {
        await GoToAsync(nameof(ChangeApiKeyPage));
    }

    public async Task<bool> DisplayAlertAsync(string title, string message, string accept = "OK", string? cancel = null)
    {
        try
        {
            if (string.IsNullOrEmpty(cancel))
            {
                await Shell.Current.CurrentPage.DisplayAlertAsync(title, message, accept);
                return true;
            }
            else
            {
                return await Shell.Current.CurrentPage.DisplayAlertAsync(title, message, accept, cancel);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to display alert");
            return false;
        }
    }

    public async Task ShowToastAsync(string message)
    {
        try
        {
            await CommunityToolkit.Maui.Alerts.Toast.Make(message, CommunityToolkit.Maui.Core.ToastDuration.Short).Show();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to show toast");
        }
    }
}