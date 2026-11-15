using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;

namespace WPFApp.Services;

/// <summary>
/// Управляет языком интерфейса
/// </summary>
public class LocalizationService
{
    private const string DictionaryFolder = "Resources/Languages/";

    /// <summary>
    /// Заменяет словарь переводов
    /// </summary>
    public void SetLanguage(string cultureName)
    {
        string fileName = cultureName switch
        {
            "ru-RU" => "ru.xaml",
            "en-US" => "en.xaml",
            "be-BY" => "bel.xaml",
            _ => throw new ArgumentOutOfRangeException(nameof(cultureName))
        };

        ResourceDictionary newDictionary = new ResourceDictionary
        {
            Source = new Uri($"{DictionaryFolder}{fileName}", UriKind.Relative)
        };

        Collection<ResourceDictionary> dictionaries = Application.Current.Resources.MergedDictionaries;
        for (int i = 0; i < dictionaries.Count; i++)
        {
            Uri? source = dictionaries[i].Source;
            if (source != null && source.OriginalString.StartsWith(DictionaryFolder, StringComparison.Ordinal))
            {
                dictionaries.RemoveAt(i);
                break;
            }
        }

        dictionaries.Add(newDictionary);

        CultureInfo culture = new CultureInfo(cultureName);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
    }
    
    public string GetString(string key)
    {
        return Application.Current.TryFindResource(key) as string ?? key;
    }
}
