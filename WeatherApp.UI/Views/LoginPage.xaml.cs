using WeatherApp.UI.ViewModels;

namespace WeatherApp.UI.Views;

public partial class LoginPage : ContentPage
{
    // Убираем аргумент конструктора! 
    // BindingContext мы пропишем в XAML, либо через MauiProgram.cs
    public LoginPage()
    {
        InitializeComponent();
    }

    // Если BindingContext не подтянулся автоматически, добавьте это:
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext == null)
        {
            // Если контекст не задан, создадим его вручную (как запасной вариант)
            BindingContext = new LoginPageViewModel();
        }
    }
}