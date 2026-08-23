using System;
using System.Collections.Generic;
using System.Text;

namespace WeatherApp.Core.Results
{
    public class ApiKeyMissingError : Error
    {
        public ApiKeyMissingError()
            : base("API_KEY_MISSING", "API ключ не найден. Пожалуйста, добавьте ключ в настройках.") { }
    }
}
