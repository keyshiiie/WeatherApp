namespace WeatherApp.Core.Models;


public class UserSettings
{
    public TemperatureUnit TemperatureUnit { get; set; } = TemperatureUnit.Celsius;
    public PressureUnit PressureUnit { get; set; } = PressureUnit.Millibars;
    public SpeedUnit SpeedUnit { get; set; } = SpeedUnit.KilometersPerHour;
    public ThemeMode ThemeMode { get; set; } = ThemeMode.System;

    public UserSettings Clone()
    {
        return new UserSettings
        {
            TemperatureUnit = this.TemperatureUnit,
            PressureUnit = this.PressureUnit,
            SpeedUnit = this.SpeedUnit,
            ThemeMode = this.ThemeMode
        };
    }

    public override bool Equals(object? obj)
    {
        if (obj is not UserSettings other)
            return false;

        return TemperatureUnit == other.TemperatureUnit &&
               PressureUnit == other.PressureUnit &&
               SpeedUnit == other.SpeedUnit &&
               ThemeMode == other.ThemeMode; 
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(TemperatureUnit, PressureUnit, SpeedUnit, ThemeMode);
    }
}

public enum ThemeMode
{
    Light,
    Dark,
    System
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