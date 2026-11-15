namespace WPFApp.Models;

/// <summary>
/// Итоги импорта CSV-файла
/// </summary>
public readonly record struct CsvImportResult(int ImportedCount, int SkippedCount);
