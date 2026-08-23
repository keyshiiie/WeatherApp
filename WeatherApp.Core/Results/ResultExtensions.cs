// WeatherApp.Core/Results/ResultExtensions.cs
namespace WeatherApp.Core.Results;

public static class ResultExtensions
{
    /// <summary>
    /// Выполняет действие в случае успеха
    /// </summary>
    public static Result<T> OnSuccess<T>(this Result<T> result, Action<T> action)
    {
        if (result.IsSuccess && result.Value != null)
        {
            action(result.Value);
        }
        return result;
    }

    /// <summary>
    /// Выполняет действие в случае ошибки
    /// </summary>
    public static Result<T> OnFailure<T>(this Result<T> result, Action<Error> action)
    {
        if (result.IsFailure && result.Error != null)
        {
            action(result.Error);
        }
        return result;
    }

    /// <summary>
    /// Преобразует результат в другой тип
    /// </summary>
    public static Result<TNew> Map<T, TNew>(this Result<T> result, Func<T, TNew> mapper)
    {
        if (result.IsSuccess && result.Value != null)
        {
            return Result.Success(mapper(result.Value));
        }
        return Result.Failure<TNew>(result.Error!);
    }

    /// <summary>
    /// Приводит к другому типу с ошибкой по умолчанию
    /// </summary>
    public static Result<TNew> Cast<T, TNew>(this Result<T> result, Error? fallbackError = null)
    {
        if (result.IsSuccess && result.Value != null)
        {
            return Result.Success((TNew)(object)result.Value);
        }
        return Result.Failure<TNew>(fallbackError ?? result.Error ?? new UnknownError("Cast failed"));
    }

    /// <summary>
    /// Получает значение или выбрасывает исключение
    /// </summary>
    public static T GetValueOrThrow<T>(this Result<T> result)
    {
        if (result.IsSuccess && result.Value != null)
        {
            return result.Value;
        }
        throw new InvalidOperationException($"Cannot get value from failed result: {result.Error}");
    }

    /// <summary>
    /// Получает значение или значение по умолчанию
    /// </summary>
    public static T GetValueOrDefault<T>(this Result<T> result, T defaultValue = default!)
    {
        return result.IsSuccess && result.Value != null ? result.Value : defaultValue;
    }
}