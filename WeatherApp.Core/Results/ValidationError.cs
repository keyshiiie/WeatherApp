using System;
using System.Collections.Generic;
using System.Text;

namespace WeatherApp.Core.Results
{
    public class ValidationError : Error
    {
        public ValidationError(string message)
            : base("VALIDATION_ERROR", message) { }
    }
}
