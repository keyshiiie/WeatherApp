using System;
using System.Collections.Generic;
using System.Text;

namespace WeatherApp.Core.Results
{
    public class UnknownError : Error
    {
        public UnknownError(string message, Exception? innerException = null)
            : base("UNKNOWN_ERROR", $"Неизвестная ошибка: {message}") { }
    }
}
