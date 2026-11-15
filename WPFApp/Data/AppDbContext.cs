using Microsoft.EntityFrameworkCore;
using WPFApp.Models;

namespace WPFApp.Data;

/// <summary>
/// Контекст базы данных для работы с записями
/// </summary>
public class AppDbContext : DbContext
{
    private readonly string _connectionString;

    public DbSet<PersonRecord> PersonRecords { get; set; } = null!;

    public AppDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(_connectionString);
    }
}