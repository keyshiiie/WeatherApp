using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WeatherApp.Core.Results;
using WeatherApp.Core.Services;
using WeatherApp.UI.Services;

namespace WeatherApp.UI.ViewModels;

public partial class ChangeApiKeyPageViewModel : BaseViewModel
{
    private readonly IApiKeyService _apiKeyService;

    [ObservableProperty]
    public partial string? NewApiKey { get; set; }
    [ObservableProperty]
    public partial string? KeyStatus { get; set; }

    [ObservableProperty]
    public partial Color KeyStatusColor { get; set; } = Colors.Gray;

    public ChangeApiKeyPageViewModel(
        IApiKeyService apiKeyService,
        INavigationService navigationService,
        ILogger<ChangeApiKeyPageViewModel> logger) 
        : base(logger, navigationService)
    {
        Title = "API ключ";
        _apiKeyService = apiKeyService ?? throw new ArgumentNullException(nameof(apiKeyService));

        // Устанавливаем начальное состояние
        KeyStatus = "Проверка...";
        KeyStatusColor = Colors.Gray;
    }

    public override async Task OnAppearingAsync()
    {
        Logger.LogInformation("ChangeApiKeyPage appearing");
        await CheckKeyStatusAsync();
    }

    private async Task CheckKeyStatusAsync()
    {
        var result = await ExecuteWithResultAsync(
            async () =>
            {
                var keyResult = await _apiKeyService.HasApiKeyAsync();
                if (keyResult.IsFailure)
                    return Result.Failure<bool>(keyResult.Error!);

                return Result.Success(keyResult.Value);
            },
            errorMessage: "Ошибка проверки ключа"
        );

        if (result.IsSuccess)
        {
            if (result.Value)
            {
                KeyStatus = "Установлен ✓";
                KeyStatusColor = Colors.Green;
            }
            else
            {
                KeyStatus = "Не установлен";
                KeyStatusColor = Colors.Red;
            }
        }
        else
        {
            KeyStatus = "Ошибка проверки";
            KeyStatusColor = Colors.Red;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(NewApiKey))
        {
            await ShowAlertAsync("Ошибка", "Введите API ключ");
            return;
        }

        Logger.LogInformation("Saving new API key...");

        var result = await ExecuteWithResultAsync(
            async () =>
            {
                var saveResult = await _apiKeyService.SetApiKeyAsync(NewApiKey.Trim());
                if (saveResult.IsFailure)
                    return Result.Failure(saveResult.Error!);

                Logger.LogInformation("API key saved successfully");
                return Result.Success();
            },
            successMessage: "API ключ успешно обновлен", // Исправлена опечатка
            errorMessage: "Не удалось сохранить API ключ"
        );

        if (result.IsSuccess)
        {
            NewApiKey = string.Empty;
            await CheckKeyStatusAsync();
            await NavigationService.GoBackAsync();
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        Logger.LogInformation("Cancelling API key change");
        await NavigationService.GoBackAsync();
    }

    [RelayCommand]
    private async Task OpenLinkAsync()
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