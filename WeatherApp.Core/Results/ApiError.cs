using System;
using System.Collections.Generic;
using System.Text;

namespace WeatherApp.Core.Results
{
    public class ApiError : Error
    {
        public int StatusCode { get; }
        public string? ResponseBody { get; }

        public ApiError(string message, int statusCode, string? responseBody = null)
            : base($"API_ERROR_{statusCode}", message)
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }
    }

}
