using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeatherApp.Core.Entities;

[Table("Cities")]
public class CityEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string? Name { get; set; }

    [Required]
    [MaxLength(200)]
    public string? Region { get; set; }

    [Required]
    [MaxLength(50)]
    public string? Country { get; set; }

    [Required]
    public double Latitude { get; set; }

    [Required]
    public double Longitude { get; set; }
    public DateTime AddedAt { get; set; }
    public bool IsLastSelected { get; set; }
    public bool IsFavorite { get; set; }
    public bool IsRecent { get; set; }
    public DateTime LastSearchedAt { get; set; }
    public virtual WeatherCacheEntity? WeatherCache { get; set; }
}