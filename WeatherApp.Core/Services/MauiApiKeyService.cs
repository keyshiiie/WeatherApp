using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;
using WeatherApp.Core.Results;

namespace WeatherApp.Core.Services;

public class MauiApiKeyService : IApiKeyService
{
    private readonly ILogger<MauiApiKeyService> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private string? _cachedKey;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);
    private DateTime _cacheTime = DateTime.MinValue;
    private const string API_KEY_STORAGE_KEY = "weather_api_key";

    public MauiApiKeyService(ILogger<MauiApiKeyService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<string>> GetApiKeyAsync()
    {
        try
        {
            if (!string.IsNullOrEmpty(_cachedKey) && DateTime.UtcNow - _cacheTime < _cacheDuration)
                return Result.Success(_cachedKey);

            await _lock.WaitAsync();
            try
            {
                if (!string.IsNullOrEmpty(_cachedKey) && DateTime.UtcNow - _cacheTime < _cacheDuration)
                    return Result.Success(_cachedKey);

                try
                {
                    _cachedKey = await SecureStorage.GetAsync(API_KEY_STORAGE_KEY);
                    _cacheTime = DateTime.UtcNow;

                    if (string.IsNullOrEmpty(_cachedKey))
                    {
                        _logger.LogWarning("API Key not found in SecureStorage");
                        return Result.Failure<string>(new ApiKeyMissingError());
                    }

                    _logger.LogInformation("API Key loaded from SecureStorage");
                    return Result.Success(_cachedKey);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to get API key from SecureStorage");
                    return Result.Failure<string>(new UnknownError("Failed to retrieve API key", ex));
                }
            }
            finally
            {
                _lock.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error getting API key");
            return Result.Failure<string>(new UnknownError("Unexpected error retrieving API key", ex));
        }
    }

    public async Task<Result> SetApiKeyAsync(string apiKey)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                return Result.Failure(new ValidationError("API key cannot be empty"));

            await _lock.WaitAsync();
            try
            {
                await SecureStorage.SetAsync(API_KEY_STORAGE_KEY, apiKey);
                _cachedKey = apiKey;
                _cacheTime = DateTime.UtcNow;
                _logger.LogInformation("API key saved successfully");
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save API key to SecureStorage");
                return Result.Failure(new UnknownError("Failed to save API key", ex));
            }
            finally
            {
                _lock.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error saving API key");
            return Result.Failure(new UnknownError("Unexpected error saving API key", ex));
        }
    }

    public async Task<Result<bool>> HasApiKeyAsync()
    {
        try
        {
            var result = await GetApiKeyAsync();
            if (result.IsSuccess)
            {
                return Result.Success(!string.IsNullOrEmpty(result.Value));
            }
            return Result.Failure<bool>(result.Error!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error checking API key existence");
            return Result.Failure<bool>(new UnknownError("Failed to check API key existence", ex));
        }
    }

    public async Task<Result> ClearApiKeyAsync()
    {
        try
        {
            await _lock.WaitAsync();
            try
            {
                SecureStorage.Remove(API_KEY_STORAGE_KEY);
                _cachedKey = null;
                _cacheTime = DateTime.MinValue;
                _logger.LogInformation("API key cleared");
                return Result.Success();
            }
            finally
            {
                _lock.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear API key");
            return Result.Failure(new UnknownError("Failed to clear API key", ex));
        }
    }
}