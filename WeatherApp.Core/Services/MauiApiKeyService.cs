using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace WeatherApp.Core.Services
{
    public class MauiApiKeyService : IApiKeyService
    {
        private readonly ILogger<MauiApiKeyService> _logger;
        private readonly SemaphoreSlim _lock = new(1, 1);
        private string? _cachedKey;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);
        private DateTime _cacheTime = DateTime.MinValue;

        private const string API_KEY_STORAGE_KEY= "weather_api_key";

        public MauiApiKeyService(ILogger<MauiApiKeyService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string?> GetApiKeyAsync()
        {
            if (!string.IsNullOrEmpty(_cachedKey) && DateTime.UtcNow - _cacheTime < _cacheDuration)
                return _cachedKey;

            await _lock.WaitAsync();
            try
            {
                if (!string.IsNullOrEmpty(_cachedKey) && DateTime.UtcNow - _cacheTime < _cacheDuration)
                    return _cachedKey;

                try
                {
                    _cachedKey = await SecureStorage.GetAsync(API_KEY_STORAGE_KEY);
                    _cacheTime = DateTime.UtcNow;

                    if (string.IsNullOrEmpty(_cachedKey))
                    {
                        _logger.LogWarning("API Key not found in SecureStorage");
                    }
                    else
                    {
                        _logger.LogInformation("API Key loaded from SecureStorage");
                    }

                    return _cachedKey;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to get API key from SecureStorage");
                    return null;
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task SetApiKeyAsync(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("API key cannot be empty", nameof(apiKey));

            await _lock.WaitAsync();
            try
            {
                await SecureStorage.SetAsync(API_KEY_STORAGE_KEY, apiKey);
                _cachedKey = apiKey;
                _cacheTime = DateTime.UtcNow;
                _logger.LogInformation("API key saved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save API key to SecureStorage");
                throw;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<bool> HasApiKeyAsync()
        {
            try
            {
                var key = await GetApiKeyAsync();
                return !string.IsNullOrEmpty(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if API key exists");
                return false;
            }
        }

        public async Task ClearApiKeyAsync()
        {
            await _lock.WaitAsync();
            try
            {
                SecureStorage.Remove(API_KEY_STORAGE_KEY);
                _cachedKey = null;
                _cacheTime = DateTime.MinValue;
                _logger.LogInformation("API key cleared");
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}
