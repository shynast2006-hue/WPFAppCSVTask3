using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using WPFApp.Data;
using WPFApp.Models;

namespace WPFApp.Services;

/// <summary>
/// Загружает записи из CSV-файла в базу данных
/// </summary>
public class CsvImportService
{
    private const int BatchSize = 5000;
    private const int FieldCount = 6;
    
    private readonly string _connectionString;

    public CsvImportService(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Заменяет записи в базе данными из CSV
    /// </summary>
    public async Task<CsvImportResult> ImportAsync(string filePath)
    {
        int importedCount = 0;
        int skippedCount = 0;
        List<PersonRecord> batch = new List<PersonRecord>(BatchSize);

        using StreamReader reader = new StreamReader(filePath, Encoding.UTF8);
        using AppDbContext context = new AppDbContext(_connectionString);

        context.Database.SetCommandTimeout(300);
        context.ChangeTracker.AutoDetectChangesEnabled = false;

        using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync();

        try
        {
            // Очищаем таблицу внутри транзакции, чтобы при сбое сохранить прежние записи
            await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE PersonRecords");

            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    skippedCount++;
                    continue;
                }

                PersonRecord? record = ParseRecord(line);
                if (record == null)
                {
                    skippedCount++;
                    continue;
                }

                batch.Add(record);
                importedCount++;

                if (batch.Count == BatchSize)
                {
                    await SaveBatchAsync(context, batch);
                }
            }

            if (importedCount == 0)
            {
                throw new InvalidDataException("В файле нет ни одной подходящей записи.");
            }

            if (batch.Count > 0)
            {
                await SaveBatchAsync(context, batch);
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        return new CsvImportResult(importedCount, skippedCount);
    }

    /// <summary>
    /// Сохраняет пакет записей и очищает его, чтобы записи не копились в памяти
    /// </summary>
    private static async Task SaveBatchAsync(AppDbContext context, List<PersonRecord> batch)
    {
        await context.PersonRecords.AddRangeAsync(batch);
        await context.SaveChangesAsync();

        batch.Clear();
        context.ChangeTracker.Clear();
    }

    /// <summary>
    /// Преобразует строку CSV в запись или возвращает null при ошибке формата
    /// </summary>
    private static PersonRecord? ParseRecord(string line)
    {
        string[] parts = line.Split(';');
        if (parts.Length != FieldCount)
        {
            return null;
        }

        if (!DateOnly.TryParseExact(parts[0].Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out DateOnly date))
        {
            return null;
        }

        for (int i = 1; i < FieldCount; i++)
        {
            parts[i] = parts[i].Trim();

            if (string.IsNullOrEmpty(parts[i]) || parts[i].Length > PersonRecord.MaxTextLength)
            {
                return null;
            }
        }

        return new PersonRecord
        {
            Date = date,
            FirstName = parts[1],
            LastName = parts[2],
            SurName = parts[3],
            City = parts[4],
            Country = parts[5]
        };
    }
}    
