using WeatherApp.Core.Models;
using WeatherApp.UI.DisplayModels;
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
                catch
                {
                    // Ignore deserialization errors
                }
            }
        }
    }

    public CurrentWeatherPage(
        CurrentWeatherViewModel viewModel,
        HourlyChartViewModel chartViewModel) // Добавляем через DI
    {
        InitializeComponent();
        BindingContext = viewModel;

        // Передаем ViewModel в HourlyChartView
        HourlyChartView.SetViewModel(chartViewModel);

        viewModel?.HourlyDataUpdated += OnHourlyDataUpdated;
    }

    protected override async void OnAppearing()
    {
        try
        {
            base.OnAppearing();
            if (BindingContext is CurrentWeatherViewModel vm)
            {
                await vm.OnAppearingAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnAppearing: {ex.Message}");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        if (BindingContext is CurrentWeatherViewModel vm)
        {
            vm.HourlyDataUpdated -= OnHourlyDataUpdated;
        }
    }

    private void OnHourlyDataUpdated(object? sender, List<HourlyForecastDisplay> data)
    {
        if (HourlyChartView == null || data == null || data.Count == 0)
            return;

        Dispatcher.Dispatch(() =>
        {
            HourlyChartView.SetData(data);

            Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), () =>
            {
                HourlyChartView.ForceRefresh();
            });
        });
    }
}