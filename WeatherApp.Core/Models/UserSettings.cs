// WeatherApp.Core/Models/UserSettings.cs
namespace WeatherApp.Core.Models;

public class UserSettings
{
    public TemperatureUnit TemperatureUnit { get; set; } = TemperatureUnit.Celsius;
    public PressureUnit PressureUnit { get; set; } = PressureUnit.Millibars;
    public SpeedUnit SpeedUnit { get; set; } = SpeedUnit.KilometersPerHour;

    /// <summary>
    /// Создает глубокую копию настроек
    /// </summary>
    public UserSettings Clone()
    {
        return new UserSettings
        {
            TemperatureUnit = this.TemperatureUnit,
            PressureUnit = this.PressureUnit,
            SpeedUnit = this.SpeedUnit
        };
    }

    /// <summary>
    /// Проверяет, эквивалентны ли настройки
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is not UserSettings other)
            return false;

        return TemperatureUnit == other.TemperatureUnit &&
               PressureUnit == other.PressureUnit &&
               SpeedUnit == other.SpeedUnit;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(TemperatureUnit, PressureUnit, SpeedUnit);
    }
}

public enum TemperatureUnit
{
    Celsius,
    Fahrenheit
}

public enum PressureUnit
{
    Millibars,
    Inches
}

public enum SpeedUnit
{
    KilometersPerHour,
    MilesPerHour
}