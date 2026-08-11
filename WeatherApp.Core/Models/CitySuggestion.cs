namespace WeatherApp.Core.Models;
public class CitySuggestion
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Region { get; set; }
    public string? Country { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Url { get; set; }

    public string DisplayText => $"{Name}, {Country}";
    public string FullDisplayText => string.IsNullOrEmpty(Region)
        ? $"{Name}, {Country}"
        : $"{Name}, {Region}, {Country}";
}