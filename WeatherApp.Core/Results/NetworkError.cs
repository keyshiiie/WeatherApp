using System;
using System.Collections.Generic;
using System.Text;

namespace WeatherApp.Core.Results
{
    public class NetworkError : Error
    {
        public NetworkError(string message = "Проблема с соединением. Проверьте интернет.")
            : base("NETWORK_ERROR", message) { }
    }
}
