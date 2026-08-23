using System;
using System.Collections.Generic;
using System.Text;

namespace WeatherApp.Core.Results
{
    public abstract class Error
    {
        public string Code { get; }
        public string Message { get; }

        protected Error(string code, string message)
        {
            Code = code;
            Message = message;
        }

        public override string ToString() => $"[{Code}] {Message}";
    }

}
