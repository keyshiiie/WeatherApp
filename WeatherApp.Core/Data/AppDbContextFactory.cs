using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WeatherApp.Core.Data;

/// Фабрика для создания AppDbContext во время дизайна (миграции)
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // Строка подключения для миграций
        optionsBuilder.UseSqlite("Data Source=weatherapp.db");

        return new AppDbContext(optionsBuilder.Options);
    }
}