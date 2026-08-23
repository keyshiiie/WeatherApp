// WeatherApp.UI/ViewModels/BaseViewModel.cs
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using WeatherApp.Core.Results;

namespace WeatherApp.UI.ViewModels;

public abstract partial class BaseViewModel : ObservableObject
{
    private string _title = string.Empty;
    private bool _isBusy;
    private string _errorMessage = string.Empty;
    private bool _hasError;

    protected ILogger Logger { get; }

    protected BaseViewModel(ILogger logger)
    {
        Logger = logger;
    }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool HasError
    {
        get => _hasError;
        set => SetProperty(ref _hasError, value);
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
        Logger.LogWarning($"Error: {error.Code} - {error.Message}");
    }

    protected void ClearError()
    {
        ErrorMessage = string.Empty;
        HasError = false;
    }

    /// <summary>
    /// Выполняет действие с обработкой Result
    /// </summary>
    protected async Task<Result<T>> ExecuteWithResultAsync<T>(
        Func<Task<Result<T>>> action,
        string successMessage = "",
        string errorMessage = "Произошла ошибка")
    {
        if (IsBusy)
            return Result.Failure<T>(new ValidationError("Операция уже выполняется"));

        try
        {
            IsBusy = true;
            ClearError();
            Logger.LogInformation($"Executing: {action.Method.Name}");

            var result = await action();

            if (result.IsSuccess)
            {
                Logger.LogInformation($"Completed: {action.Method.Name} - Success");
                if (!string.IsNullOrEmpty(successMessage))
                {
                    await ShowToastAsync(successMessage);
                }
                return result;
            }
            else
            {
                Logger.LogWarning($"Completed: {action.Method.Name} - Failed: {result.Error?.Message}");
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
            Logger.LogError(ex, $"Error in {action.Method.Name}: {errorMessage}");
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

    /// <summary>
    /// Выполняет действие без возврата результата
    /// </summary>
    protected async Task<Result> ExecuteWithResultAsync(
        Func<Task<Result>> action,
        string successMessage = "",
        string errorMessage = "Произошла ошибка")
    {
        if (IsBusy)
            return Result.Failure(new ValidationError("Операция уже выполняется"));

        try
        {
            IsBusy = true;
            ClearError();
            Logger.LogInformation($"Executing: {action.Method.Name}");

            var result = await action();

            if (result.IsSuccess)
            {
                Logger.LogInformation($"Completed: {action.Method.Name} - Success");
                if (!string.IsNullOrEmpty(successMessage))
                {
                    await ShowToastAsync(successMessage);
                }
                return result;
            }
            else
            {
                Logger.LogWarning($"Completed: {action.Method.Name} - Failed: {result.Error?.Message}");
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
            Logger.LogError(ex, $"Error in {action.Method.Name}: {errorMessage}");
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

    /// <summary>
    /// Показывает Toast сообщение
    /// </summary>
    protected virtual async Task ShowToastAsync(string message)
    {
        try
        {
            await CommunityToolkit.Maui.Alerts.Toast.Make(message, CommunityToolkit.Maui.Core.ToastDuration.Short).Show();
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to show toast");
        }
    }

    /// <summary>
    /// Показывает Alert сообщение
    /// </summary>
    protected virtual async Task ShowAlertAsync(string title, string message)
    {
        try
        {
            await Shell.Current.CurrentPage.DisplayAlertAsync(title, message, "OK");
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to show alert");
        }
    }

    /// <summary>
    /// Обрабатывает ошибку Result и возвращает понятное сообщение
    /// </summary>
    protected string GetUserFriendlyErrorMessage(Error error)
    {
        return error switch
        {
            ApiKeyMissingError => "⚠️ API ключ не найден. Добавьте его в настройках.",
            NetworkError => "🌐 Нет подключения к интернету. Проверьте соединение.",
            TimeoutError => "⏱️ Превышено время ожидания. Попробуйте еще раз.",
            NotFoundError notFound => $"🔍 {notFound.Message}",
            ApiError apiError when apiError.StatusCode == 401 => "🔑 Неверный API ключ. Проверьте настройки.",
            ApiError apiError => $"❌ Ошибка API: {apiError.Message}",
            DatabaseError dbError => $"💾 Ошибка базы данных: {dbError.Message}",
            ValidationError validationError => $"⚠️ {validationError.Message}",
            _ => $"❌ {error.Message}"
        };
    }
}