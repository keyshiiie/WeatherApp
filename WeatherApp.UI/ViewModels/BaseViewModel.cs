using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WeatherApp.UI.ViewModels;

public abstract partial class BaseViewModel : ObservableObject
{
    private string _title = string.Empty;
    private bool _isBusy;
    private string _errorMessage = string.Empty;
    private bool _hasError;

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

    protected void ClearError()
    {
        ErrorMessage = string.Empty;
        HasError = false;
    }

    protected async Task ExecuteAsync(Func<Task> action, string errorMessage = "Произошла ошибка")
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            ClearError();
            await action();
        }
        catch (Exception ex)
        {
            SetError($"{errorMessage}: {ex.Message}");
            // Здесь можно добавить логирование
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected async Task<T> ExecuteAsync<T>(Func<Task<T>> action, string errorMessage = "Произошла ошибка")
    {
        if (IsBusy)
            return default!;

        try
        {
            IsBusy = true;
            ClearError();
            return await action();
        }
        catch (Exception ex)
        {
            SetError($"{errorMessage}: {ex.Message}");
            return default!;
        }
        finally
        {
            IsBusy = false;
        }
    }
}