using System;
using System.Collections.Generic;
using System.Text;
using WeatherApp.Core.Models;

namespace WeatherApp.UI.Services
{
    public interface INavigationService
    {
        Task GoToAsync(string route);
        Task GoToAsync(string route, IDictionary<string, object> parameters);
        Task GoBackAsync();
        Task GoToMainPageAsync();
        Task GoToWeatherPageAsync(City city);
        Task GoToWeatherPageAsync(int cityId);
        Task GoToFavoritesPageAsync();
        Task GoToSettingsPageAsync();
        Task ShowLoginModalAsync();
        Task GoToChangeApiKeyPageAsync();
        Task<bool> DisplayAlertAsync(string title, string message, string accept = "OK", string? cancel = null);
        Task ShowToastAsync(string message);
    }
}
