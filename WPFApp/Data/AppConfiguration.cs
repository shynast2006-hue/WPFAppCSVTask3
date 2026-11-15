using Microsoft.Extensions.Configuration;

namespace WPFApp.Data;

/// <summary>
/// Читает строку подключения из appsettings.json
/// </summary>
public static class AppConfiguration
{
    public static string GetConnectionString()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        string? connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "В appsettings.json не задана строка подключения DefaultConnection.");
        }

        return connectionString;
    }
}


