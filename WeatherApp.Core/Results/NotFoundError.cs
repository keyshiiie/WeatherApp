using System;
using System.Collections.Generic;
using System.Text;

namespace WeatherApp.Core.Results
{
    public class NotFoundError : Error
    {
        public NotFoundError(string entityType, string identifier)
            : base("NOT_FOUND", $"{entityType} '{identifier}' не найден.") { }
    }
}
