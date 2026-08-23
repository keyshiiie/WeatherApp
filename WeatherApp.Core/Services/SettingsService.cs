// WeatherApp.Core/Services/SettingsService.cs
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;
using WeatherApp.Core.Models;
using WeatherApp.Core.Results;

namespace WeatherApp.Core.Services;

public class SettingsService : ISettingsService
{
    private const string TemperatureKey = "temperature_unit";
    private const string PressureKey = "pressure_unit";
    private const string SpeedKey = "speed_unit";

    private UserSettings _cachedSettings;
    private readonly object _lock = new();
    private readonly ILogger<SettingsService>? _logger;

    public event EventHandler<UserSettings>? SettingsChanged;

    public SettingsService(ILogger<SettingsService>? logger = null)
    {
        _logger = logger;
        var loadResult = LoadSettings();

        if (loadResult.IsSuccess)
        {
            _cachedSettings = loadResult.Value!;
            _logger?.LogInformation("Settings loaded successfully");
        }
        else
        {
            _logger?.LogWarning($"Failed to load settings: {loadResult.Error?.Message}");
            _cachedSettings = new UserSettings();
        }
    }

    public Result<UserSettings> GetSettings()
    {
        try
        {
            lock (_lock)
            {
                // Возвращаем КОПИЮ, чтобы нельзя было изменить кэш извне
                return Result.Success(_cachedSettings.Clone());
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to get settings");
            return Result.Failure<UserSettings>(
                new UnknownError("Failed to retrieve settings", ex));
        }
    }

    public Result SaveSettings(UserSettings settings)
    {
        try
        {
            if (settings == null)
                return Result.Failure(new ValidationError("Settings cannot be null"));

            var settingsCopy = settings.Clone();

            lock (_lock)
            {
                Preferences.Set(TemperatureKey, (int)settingsCopy.TemperatureUnit);
                Preferences.Set(PressureKey, (int)settingsCopy.PressureUnit);
                Preferences.Set(SpeedKey, (int)settingsCopy.SpeedUnit);

                _cachedSettings = settingsCopy;
            }

            _logger?.LogInformation($"Settings saved: Temp={settingsCopy.TemperatureUnit}, " +
                                   $"Pressure={settingsCopy.PressureUnit}, " +
                                   $"Speed={settingsCopy.SpeedUnit}");

            // Вызываем событие ПОСЛЕ сохранения
            SettingsChanged?.Invoke(this, settingsCopy);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to save settings");
            return Result.Failure(new UnknownError("Failed to save settings", ex));
        }
    }

    public Result SetTemperatureUnit(TemperatureUnit unit)
    {
        try
        {
            var settingsResult = GetSettings();
            if (settingsResult.IsFailure)
                return Result.Failure(settingsResult.Error!);

            var settings = settingsResult.Value!;

            // Проверяем, изменилось ли значение
            if (settings.TemperatureUnit == unit)
                return Result.Success(); // Ничего не меняем

            settings.TemperatureUnit = unit;
            return SaveSettings(settings);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Failed to set temperature unit to {unit}");
            return Result.Failure(new UnknownError($"Failed to set temperature unit to {unit}", ex));
        }
    }

    public Result SetPressureUnit(PressureUnit unit)
    {
        try
        {
            var settingsResult = GetSettings();
            if (settingsResult.IsFailure)
                return Result.Failure(settingsResult.Error!);

            var settings = settingsResult.Value!;

            if (settings.PressureUnit == unit)
                return Result.Success();

            settings.PressureUnit = unit;
            return SaveSettings(settings);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Failed to set pressure unit to {unit}");
            return Result.Failure(new UnknownError($"Failed to set pressure unit to {unit}", ex));
        }
    }

    public Result SetSpeedUnit(SpeedUnit unit)
    {
        try
        {
            var settingsResult = GetSettings();
            if (settingsResult.IsFailure)
                return Result.Failure(settingsResult.Error!);

            var settings = settingsResult.Value!;

            if (settings.SpeedUnit == unit)
                return Result.Success();

            settings.SpeedUnit = unit;
            return SaveSettings(settings);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Failed to set speed unit to {unit}");
            return Result.Failure(new UnknownError($"Failed to set speed unit to {unit}", ex));
        }
    }

    private Result<UserSettings> LoadSettings()
    {
        try
        {
            var settings = new UserSettings
            {
                TemperatureUnit = (TemperatureUnit)Preferences.Get(
                    TemperatureKey,
                    (int)TemperatureUnit.Celsius),

                PressureUnit = (PressureUnit)Preferences.Get(
                    PressureKey,
                    (int)PressureUnit.Millibars),

                SpeedUnit = (SpeedUnit)Preferences.Get(
                    SpeedKey,
                    (int)SpeedUnit.KilometersPerHour)
            };

            return Result.Success(settings);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to load settings from preferences");
            return Result.Failure<UserSettings>(
                new UnknownError("Failed to load settings", ex));
        }
    }
}