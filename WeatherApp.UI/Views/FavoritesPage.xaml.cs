using WeatherApp.UI.ViewModels;

namespace WeatherApp.UI.Views;

public partial class FavoritesPage : ContentPage
{
    public FavoritesPage(FavoritesPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is FavoritesPageViewModel vm)
        {
            await vm.LoadFavoritesCommand.ExecuteAsync(null);
        }
    }
}