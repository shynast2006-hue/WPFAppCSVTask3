using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace WPFApp.Models;

/// <summary>
/// Запись, импортированная из CSV-файла
/// </summary>
// Индексы для сортировки записей и поиска по фамилии и городу
[Index(nameof(Date), nameof(LastName), nameof(FirstName))]
[Index(nameof(LastName))]
[Index(nameof(City))]
public class PersonRecord
{
    public const int MaxTextLength = 100;

    public int Id { get; set; }

    public DateOnly Date { get; set; }

    [MaxLength(MaxTextLength)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(MaxTextLength)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(MaxTextLength)]
    public string SurName { get; set; } = string.Empty;

    [MaxLength(MaxTextLength)]
    public string City { get; set; } = string.Empty;

    [MaxLength(MaxTextLength)]
    public string Country { get; set; } = string.Empty;
}
