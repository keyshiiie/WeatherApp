using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WeatherApp.Core.Services;
using WeatherApp.Core.Results;
using WeatherApp.UI.Services;

namespace WeatherApp.UI.ViewModels;

public partial class LoginPageViewModel : BaseViewModel
{
    private readonly IApiKeyService _apiKeyService;

    [ObservableProperty]
    public partial string? ApiKey { get; set; }

    [ObservableProperty]
    public partial bool IsValidating { get; set; }

    public LoginPageViewModel(
        IApiKeyService apiKeyService,
        INavigationService navigationService, 
        ILogger<LoginPageViewModel> logger)
        : base(logger, navigationService) 
    {
        Title = "Вход";
        _apiKeyService = apiKeyService ?? throw new ArgumentNullException(nameof(apiKeyService));
        Logger.LogInformation("LoginPageViewModel initialized");
    }

    public override async Task OnAppearingAsync()
    {
        Logger.LogInformation("Login page appearing");

        var result = await ExecuteWithResultAsync(
            async () =>
            {
                var keyResult = await _apiKeyService.HasApiKeyAsync();
                if (keyResult.IsFailure)
                    return Result.Failure<bool>(keyResult.Error!);

                if (keyResult.Value)
                {
                    Logger.LogInformation("Existing API key found");
                    await ShowToastAsync("API ключ уже установлен");
                }

                return Result.Success(keyResult.Value);
            },
            errorMessage: "Не удалось проверить сохраненный ключ"
        );
    }

    [RelayCommand]
    private async Task Login()
    {
        Logger.LogInformation("Login attempt");

        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            Logger.LogWarning("Login failed: API key is empty");
            await ShowAlertAsync("Ошибка", "Введите API ключ!");
            return;
        }

        var trimmedKey = ApiKey.Trim();

        var result = await ExecuteWithResultAsync(
            async () =>
            {
                IsValidating = true;

                var saveResult = await _apiKeyService.SetApiKeyAsync(trimmedKey);
                if (saveResult.IsFailure)
                    return Result.Failure(saveResult.Error!);

                Logger.LogInformation("API key saved successfully");
                return Result.Success();
            },
            successMessage: "API ключ сохранен",
            errorMessage: "Не удалось сохранить API ключ"
        );

        if (result.IsSuccess)
        {
            await NavigationService.GoBackAsync(); 
        }

        IsValidating = false;
    }

    [RelayCommand]
    private async Task OpenLink()
    {
        Logger.LogInformation("Opening WeatherAPI signup link");

        try
        {
            await Launcher.Default.OpenAsync("https://www.weatherapi.com/signup.aspx");
            Logger.LogInformation("Link opened successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error opening link");
            await ShowAlertAsync("Ошибка", "Не удалось открыть ссылку");
        }
    }
}