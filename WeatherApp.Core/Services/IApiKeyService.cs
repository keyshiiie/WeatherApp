using System;
using System.Collections.Generic;
using System.Text;

namespace WeatherApp.Core.Services
{
    public interface IApiKeyService
    {
        Task<string?> GetApiKeyAsync();
        Task SetApiKeyAsync(string apiKey);
        Task<bool> HasApiKeyAsync();
        Task ClearApiKeyAsync();
    }
}
