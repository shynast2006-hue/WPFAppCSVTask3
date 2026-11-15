namespace WPFApp.Models;

/// <summary>
/// Страница результатов поиска и общее число совпадений
/// </summary>
public readonly record struct RecordSearchResult(List<PersonRecord> Records, int TotalCount);
