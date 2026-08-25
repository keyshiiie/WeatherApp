using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using WeatherApp.Core.Results;
using WeatherApp.UI.Services;

namespace WeatherApp.UI.ViewModels;

public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool HasError { get; set; }

    protected ILogger Logger { get; }
    protected INavigationService NavigationService { get; }

    protected BaseViewModel(ILogger logger,
        INavigationService navigationService)
    {
        Logger = logger;
        NavigationService = navigationService;
    }

    public virtual Task OnAppearingAsync()
    {
        return Task.CompletedTask;
    }

    public virtual Task OnDisappearingAsync()
    {
        return Task.CompletedTask;
    }

    protected void SetError(string message)
    {
        ErrorMessage = message;
        HasError = !string.IsNullOrEmpty(message);
    }

    protected void SetError(Error error)
    {
        ErrorMessage = error.Message;
        HasError = true;
        Logger.LogWarning("Error: {ErrorCode} - {ErrorMessage}", error.Code, error.Message);
    }

    protected void ClearError()
    {
        ErrorMessage = string.Empty;
        HasError = false;
    }

    protected async Task<Result<T>> ExecuteWithResultAsync<T>(
        Func<Task<Result<T>>> action,
        string successMessage = "",
        string errorMessage = "Произошла ошибка")
    {
        if (IsBusy)
            return Result.Failure<T>(new ValidationError("Операция уже выполняется"));

        var methodName = action.Method.Name;

        try
        {
            IsBusy = true;
            ClearError();
            Logger.LogInformation("Executing: {MethodName}", methodName);

            var result = await action();

            if (result.IsSuccess)
            {
                Logger.LogInformation("Completed: {MethodName} - Success", methodName);
                if (!string.IsNullOrEmpty(successMessage))
                {
                    await ShowToastAsync(successMessage);
                }
                return result;
            }
            else
            {
                Logger.LogWarning("Completed: {MethodName} - Failed: {ErrorMessage}",
                    methodName, result.Error?.Message);
                SetError(result.Error!);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    await ShowToastAsync($"{errorMessage}: {result.Error?.Message}");
                }
                return Result.Failure<T>(result.Error!);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in {MethodName}: {ErrorMessage}", methodName, errorMessage);
            var error = new UnknownError($"{errorMessage}: {ex.Message}", ex);
            SetError(error);
            await ShowToastAsync(error.Message);
            return Result.Failure<T>(error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected async Task<Result> ExecuteWithResultAsync(
        Func<Task<Result>> action,
        string successMessage = "",
        string errorMessage = "Произошла ошибка")
    {
        if (IsBusy)
            return Result.Failure(new ValidationError("Операция уже выполняется"));

        var methodName = action.Method.Name;

        try
        {
            IsBusy = true;
            ClearError();
            Logger.LogInformation("Executing: {MethodName}", methodName);

            var result = await action();

            if (result.IsSuccess)
            {
                Logger.LogInformation("Completed: {MethodName} - Success", methodName);
                if (!string.IsNullOrEmpty(successMessage))
                {
                    await ShowToastAsync(successMessage);
                }
                return result;
            }
            else
            {
                Logger.LogWarning("Completed: {MethodName} - Failed: {ErrorMessage}",
                    methodName, result.Error?.Message);
                SetError(result.Error!);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    await ShowToastAsync($"{errorMessage}: {result.Error?.Message}");
                }
                return Result.Failure(result.Error!);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in {MethodName}: {ErrorMessage}", methodName, errorMessage);
            var error = new UnknownError($"{errorMessage}: {ex.Message}", ex);
            SetError(error);
            await ShowToastAsync(error.Message);
            return Result.Failure(error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected async Task ShowToastAsync(string message)
    {
        await NavigationService.ShowToastAsync(message);
    }

    protected async Task<bool> ShowAlertAsync(string title, string message, string accept = "OK", string? cancel = null)
    {
        return await NavigationService.DisplayAlertAsync(title, message, accept, cancel);
    }

    protected static string GetUserFriendlyErrorMessage(Error error) => error switch
    {
        ApiKeyMissingError => "API ключ не найден. Добавьте его в настройках.",
        NetworkError => "Нет подключения к интернету. Проверьте соединение.",
        TimeoutError => "Превышено время ожидания. Попробуйте еще раз.",
        NotFoundError notFound => $"{notFound.Message}",
        ApiError apiError when apiError.StatusCode == 401 => "Неверный API ключ. Проверьте настройки.",
        ApiError apiError => $"Ошибка API: {apiError.Message}",
        DatabaseError dbError => $"Ошибка базы данных: {dbError.Message}",
        ValidationError validationError => $"{validationError.Message}",
        _ => $"{error.Message}"
    };
}