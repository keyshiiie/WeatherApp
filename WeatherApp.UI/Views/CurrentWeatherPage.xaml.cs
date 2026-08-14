using WeatherApp.Core.Models;
using WeatherApp.UI.ViewModels;

namespace WeatherApp.UI.Views;

[QueryProperty(nameof(CityJson), "city")]
public partial class CurrentWeatherPage : ContentPage
{
    private string _cityJson = string.Empty;
    public string CityJson
    {
        get => _cityJson;
        set
        {
            _cityJson = value ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(_cityJson) && _cityJson != "{}" && BindingContext is CurrentWeatherViewModel vm)
            {
                try
                {
                    var city = System.Text.Json.JsonSerializer.Deserialize<City>(_cityJson);
                    if (city != null && !string.IsNullOrEmpty(city.Name))
                    {
                        _ = vm.LoadWeatherForCityAsync(city);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка десериализации: {ex.Message}");
                }
            }
        }
    }

    public CurrentWeatherPage(CurrentWeatherViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is CurrentWeatherViewModel vm)
        {
            await vm.OnAppearingAsync();
        }
    }
}