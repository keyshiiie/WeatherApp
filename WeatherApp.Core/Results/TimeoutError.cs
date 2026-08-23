using System;
using System.Collections.Generic;
using System.Text;

namespace WeatherApp.Core.Results
{
    public class TimeoutError : Error
    {
        public TimeoutError(string message = "Превышено время ожидания ответа от сервера.")
            : base("TIMEOUT_ERROR", message) { }
    }
}
