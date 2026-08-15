using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Storage;

namespace WeatherApp.UI.ViewModels;

public partial class LoginPageViewModel : ObservableObject
{
    [ObservableProperty]
    private string? _apiKey;

    [RelayCommand]
    private async Task Login()
    {
        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            await Shell.Current.CurrentPage.DisplayAlertAsync("Ошибка", "Введите API ключ!", "ОК");
            return;
        }

        await SecureStorage.SetAsync("weather_api_key", ApiKey.Trim());
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task OpenLink()
    {
        await Launcher.Default.OpenAsync("https://www.weatherapi.com/signup.aspx");
    }
}