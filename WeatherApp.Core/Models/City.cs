using System;

namespace WeatherApp.Core.Models;
public class City
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Country { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime AddedAt { get; set; }
    public bool IsLastSelected { get; set; }

    public string DisplayName => $"{Name}, {Country}";
}