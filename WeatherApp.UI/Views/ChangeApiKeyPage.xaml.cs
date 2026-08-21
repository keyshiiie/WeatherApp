using WeatherApp.UI.ViewModels;

namespace WeatherApp.UI.Views;

public partial class ChangeApiKeyPage : ContentPage
{
    public ChangeApiKeyPage(ChangeApiKeyPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}