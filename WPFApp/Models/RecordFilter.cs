namespace WPFApp.Models;

/// <summary>
/// Условия поиска записей
/// </summary>
public class RecordFilter
{
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? SurName { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }

    /// <summary>
    /// Проверяет, что дата начала не позже даты окончания, если указаны обе
    /// </summary>
    public bool HasValidDateRange()
    {
        if (FromDate.HasValue && ToDate.HasValue)
        {
            return FromDate.Value <= ToDate.Value;
        }

        return true;
    }
}
