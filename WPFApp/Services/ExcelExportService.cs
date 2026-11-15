using System.Globalization;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using WPFApp.Models;

namespace WPFApp.Services;

/// <summary>
/// Экспорт записей в Excel
/// </summary>
public class ExcelExportService
{
    public void Export(
        string filePath,
        IEnumerable<PersonRecord> records,
        string[] columnNames,
        string sheetName)
    {
        using SpreadsheetDocument document = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook);
        WorkbookPart workbookPart = document.AddWorkbookPart();
        workbookPart.Workbook = new Workbook();
        WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();

        Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());
        sheets.Append(new Sheet
        {
            Id = workbookPart.GetIdOfPart(worksheetPart),
            SheetId = 1,
            Name = sheetName
        });

        using OpenXmlWriter writer = OpenXmlWriter.Create(worksheetPart);
        writer.WriteStartElement(new Worksheet());
        writer.WriteStartElement(new SheetData());

        // Записываем строку заголовков
        writer.WriteStartElement(new Row());
        foreach (string columnName in columnNames)
        {
            WriteCell(writer, columnName);
        }
        writer.WriteEndElement();

        // Записываем данные
        foreach (PersonRecord record in records)
        {
            writer.WriteStartElement(new Row());
            WriteCell(writer, record.Date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture));
            WriteCell(writer, record.FirstName);
            WriteCell(writer, record.LastName);
            WriteCell(writer, record.SurName);
            WriteCell(writer, record.City);
            WriteCell(writer, record.Country);
            writer.WriteEndElement();
        }

        writer.WriteEndElement(); // SheetData
        writer.WriteEndElement(); // Worksheet
        workbookPart.Workbook.Save();
    }

    private static void WriteCell(OpenXmlWriter writer, string value)
    {
        writer.WriteElement(new Cell
        {
            DataType = CellValues.String,
            CellValue = new CellValue(value)
        });
    }
}

