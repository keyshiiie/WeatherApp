using System;
using System.Collections.Generic;
using System.Text;

namespace WeatherApp.Core.Results
{
    public class DatabaseError : Error
    {
        public DatabaseError(string message, Exception? innerException = null)
            : base("DATABASE_ERROR", $"Ошибка базы данных: {message}") { }
    }

}
