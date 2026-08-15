namespace WeatherApp.Core.Models;

public class UserSettings
{
    public TemperatureUnit TemperatureUnit { get; set; } = TemperatureUnit.Celsius;
    public PressureUnit PressureUnit { get; set; } = PressureUnit.Millibars;
    public SpeedUnit SpeedUnit { get; set; } = SpeedUnit.KilometersPerHour;
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