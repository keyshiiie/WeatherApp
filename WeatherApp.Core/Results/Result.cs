using System;
using System.Collections.Generic;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WeatherApp.Core.Results
{
    public abstract class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public Error? Error { get; }

        protected Result(bool isSuccess, Error? error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result Success() => new SuccessResult();
        public static Result Failure(Error error) => new FailureResult(error);

        public static Result<T> Success<T>(T value) => new SuccessResult<T>(value);
        public static Result<T> Failure<T>(Error error) => new FailureResult<T>(error);
    }

    /// <summary>
    /// Типизированный результат с данными
    /// </summary>
    public abstract class Result<T> : Result
    {
        public abstract T? Value { get; }

        protected Result(bool isSuccess, Error? error) : base(isSuccess, error) { }

        public static implicit operator Result<T>(T value) => Success(value);
        public static implicit operator Result<T>(Error error) => Failure<T>(error);
    }

    // Внутренние классы для удобства
    internal class SuccessResult : Result
    {
        public SuccessResult() : base(true, null) { }
    }

    internal class FailureResult : Result
    {
        public FailureResult(Error error) : base(false, error) { }
    }

    internal class SuccessResult<T> : Result<T>
    {
        private readonly T _value;
        public override T? Value => _value;

        public SuccessResult(T value) : base(true, null)
        {
            _value = value;
        }
    }

    internal class FailureResult<T> : Result<T>
    {
        public override T? Value => default;

        public FailureResult(Error error) : base(false, error) { }
    }
}
