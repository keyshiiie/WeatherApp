using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeatherApp.Core.Entities;

/// Сущность для кэширования данных о погоде в БД
[Table("WeatherCache")]
public class WeatherCacheEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int CityId { get; set; }

    [Required]
    [MaxLength(100)]
    public string? CityName { get; set; }

    [Required]
    public double Latitude { get; set; }

    [Required]
    public double Longitude { get; set; }

    /// Сериализованный JSON с данными погоды
    [Required]
    [Column(TypeName = "TEXT")]
    public string? WeatherDataJson { get; set; }

    public DateTime CachedAt { get; set; }

    /// Время жизни кэша (30 минут)
    public DateTime ExpiresAt { get; set; }

    /// Проверка, актуален ли кэш
    [NotMapped]
    public bool IsValid => DateTime.UtcNow < ExpiresAt;

    // Навигационное свойство
    [ForeignKey(nameof(CityId))]
    public virtual CityEntity? City { get; set; }
}