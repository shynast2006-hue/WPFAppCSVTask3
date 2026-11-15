using Microsoft.EntityFrameworkCore;
using WPFApp.Data;
using WPFApp.Models;

namespace WPFApp.Services;

/// <summary>
/// Выполняет поиск записей в базе данных
/// </summary>
public class RecordService
{
    private readonly string _connectionString;

    public RecordService(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Возвращает страницу результатов и общее число совпадений
    /// </summary>
    public async Task<RecordSearchResult> GetPageAsync(RecordFilter filter, int pageNumber, int pageSize)
    {
        using AppDbContext context = new AppDbContext(_connectionString);
        IQueryable<PersonRecord> query = ApplyFilter(context.PersonRecords.AsNoTracking(), filter);

        int totalCount = await query.CountAsync();
        List<PersonRecord> records = await OrderRecords(query)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new RecordSearchResult(records, totalCount);
    }

    /// <summary>
    /// Возвращает записи для экспорта без загрузки всего результата в список
    /// </summary>
    public IEnumerable<PersonRecord> GetRecordsForExport(RecordFilter filter)
    {
        using AppDbContext context = new AppDbContext(_connectionString);
        IQueryable<PersonRecord> query = OrderRecords(ApplyFilter(context.PersonRecords.AsNoTracking(), filter));

        foreach (PersonRecord record in query)
        {
            yield return record;
        }
    }

    /// <summary>
    /// Добавляет условия для заполненных полей фильтра
    /// </summary>
    private static IQueryable<PersonRecord> ApplyFilter(IQueryable<PersonRecord> query, RecordFilter filter)
    {
        if (filter.FromDate.HasValue)
        {
            DateOnly fromDate = filter.FromDate.Value;
            query = query.Where(record => record.Date >= fromDate);
        }

        if (filter.ToDate.HasValue)
        {
            DateOnly toDate = filter.ToDate.Value;
            query = query.Where(record => record.Date <= toDate);
        }

        if (!string.IsNullOrWhiteSpace(filter.FirstName))
        {
            string firstName = filter.FirstName.Trim();
            query = query.Where(record => record.FirstName == firstName);
        }

        if (!string.IsNullOrWhiteSpace(filter.LastName))
        {
            string lastName = filter.LastName.Trim();
            query = query.Where(record => record.LastName == lastName);
        }

        if (!string.IsNullOrWhiteSpace(filter.SurName))
        {
            string surName = filter.SurName.Trim();
            query = query.Where(record => record.SurName == surName);
        }

        if (!string.IsNullOrWhiteSpace(filter.City))
        {
            string city = filter.City.Trim();
            query = query.Where(record => record.City == city);
        }

        if (!string.IsNullOrWhiteSpace(filter.Country))
        {
            string country = filter.Country.Trim();
            query = query.Where(record => record.Country == country);
        }

        return query;
    }

    private static IQueryable<PersonRecord> OrderRecords(IQueryable<PersonRecord> query)
    {
        return query
            .OrderBy(record => record.Date)
            .ThenBy(record => record.LastName)
            .ThenBy(record => record.FirstName)
            .ThenBy(record => record.Id);
    }
}
