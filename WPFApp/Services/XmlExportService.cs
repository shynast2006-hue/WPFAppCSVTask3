using System.Globalization;
using System.Xml;
using WPFApp.Models;

namespace WPFApp.Services;

/// <summary>
/// Экспорт записей в формат XML
/// </summary>
public class XmlExportService
{
    public void Export(string filePath, IEnumerable<PersonRecord> records)
    {
        XmlWriterSettings settings = new XmlWriterSettings
        {
            Indent = true
        };

        using XmlWriter writer = XmlWriter.Create(filePath, settings);
        writer.WriteStartDocument();
        writer.WriteStartElement("TestProgram");

        int id = 1;
        foreach (PersonRecord record in records)
        {
            writer.WriteStartElement("Record");
            writer.WriteAttributeString("id", id.ToString());

            writer.WriteElementString("Date", record.Date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture));
            writer.WriteElementString("FirstName", record.FirstName);
            writer.WriteElementString("LastName", record.LastName);
            writer.WriteElementString("SurName", record.SurName);
            writer.WriteElementString("City", record.City);
            writer.WriteElementString("Country", record.Country);

            writer.WriteEndElement();
            id++;
        }

        writer.WriteEndElement();
        writer.WriteEndDocument();
    }
}

