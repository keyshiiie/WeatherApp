using Microsoft.Maui.Storage;
using System.Windows.Input;

namespace WeatherApp.UI.ViewModels;

public class LoginPageViewModel : BindableObject
{
    private string _apiKey;

    public string ApiKey
    {
        get => _apiKey;
        set { _apiKey = value; OnPropertyChanged(); }
    }

    public ICommand LoginCommand { get; }
    public ICommand OpenLinkCommand { get; }

    public LoginPageViewModel()
    {
        LoginCommand = new Command(async () => await Login());
        OpenLinkCommand = new Command(async () => await OpenLink());
    }

    private async Task Login()
    {
        var key = ApiKey?.Trim();

        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            await Application.Current.MainPage.DisplayAlert("Ошибка", "Введите API ключ!", "ОК");
            return;
        }

        // Сохраняем ключ
        await SecureStorage.SetAsync("weather_api_key", ApiKey.Trim());

        // ВАШ ПРИМЕР: Используем GoToAsync
        // Возвращаемся назад (на главную страницу), так как мы зашли в LoginPage через навигацию
        await Shell.Current.GoToAsync("..");
    }

    private async Task OpenLink()
    {
        await Launcher.Default.OpenAsync("https://www.weatherapi.com/signup.aspx");
    }
}