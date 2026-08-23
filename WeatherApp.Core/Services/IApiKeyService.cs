using System;
using System.Collections.Generic;
using System.Text;
using WeatherApp.Core.Results;

namespace WeatherApp.Core.Services
{
    public interface IApiKeyService
    {
        Task<Result<string>> GetApiKeyAsync();
        Task<Result> SetApiKeyAsync(string apiKey);
        Task<Result<bool>> HasApiKeyAsync();
        Task<Result> ClearApiKeyAsync();
    }
}
