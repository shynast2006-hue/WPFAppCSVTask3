using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WPFApp.Commands;
using WPFApp.Models;
using WPFApp.Services;

namespace WPFApp.ViewModels;

/// <summary>
/// Состояние главного окна и действия пользователя
/// </summary>
public class MainWindowViewModel : INotifyPropertyChanged
{
    private const int PageSize = 20;

    private readonly CsvImportService _csvImportService;
    private readonly RecordService _recordService;
    private readonly ExcelExportService _excelExportService;
    private readonly XmlExportService _xmlExportService;
    private readonly IDialogService _dialogService;
    private readonly LocalizationService _localizationService;

    private RecordFilter _currentFilter = new RecordFilter();
    private int _currentPage = 1;
    private int _totalRecords;

    private LanguageOption _selectedLanguage;
    private string? _csvPath;
    private DateTime? _fromDate;
    private DateTime? _toDate;
    private string? _firstName;
    private string? _lastName;
    private string? _surName;
    private string? _city;
    private string? _country;
    private string _statusText = string.Empty;
    private string _pageText = string.Empty;
    private bool _isBusy;

    public event PropertyChangedEventHandler? PropertyChanged;

    public MainWindowViewModel(
        CsvImportService csvImportService,
        RecordService recordService,
        ExcelExportService excelExportService,
        XmlExportService xmlExportService,
        IDialogService dialogService,
        LocalizationService localizationService)
    {
        _csvImportService = csvImportService;
        _recordService = recordService;
        _excelExportService = excelExportService;
        _xmlExportService = xmlExportService;
        _dialogService = dialogService;
        _localizationService = localizationService;

        Languages = new List<LanguageOption>
        {
            new LanguageOption("ru-RU", "Русский"),
            new LanguageOption("en-US", "English"),
            new LanguageOption("be-BY", "Беларуская")
        };
        _selectedLanguage = Languages[0];
        _localizationService.SetLanguage(_selectedLanguage.Code);
        
        ChooseCsvCommand = new RelayCommand(_ => ChooseCsv());
        ImportCommand = new RelayCommand(async _ => await ImportAsync(), _ => CsvPath != null);
        SearchCommand = new RelayCommand(async _ => await SearchAsync());
        PreviousPageCommand = new RelayCommand(
            async _ => await LoadPageAsync(_currentPage - 1),
            _ => _currentPage > 1);
        NextPageCommand = new RelayCommand(
            async _ => await LoadPageAsync(_currentPage + 1),
            _ => _currentPage < GetPageCount());
        ExportExcelCommand = new RelayCommand(async _ => await ExportExcelAsync(), _ => _totalRecords > 0);
        ExportXmlCommand = new RelayCommand(async _ => await ExportXmlAsync(), _ => _totalRecords > 0);

        StatusText = Text("StatusChooseCsv");
        UpdatePageText();
    }

    public ObservableCollection<PersonRecord> Records { get; } = new ObservableCollection<PersonRecord>();
    public List<LanguageOption> Languages { get; }

    public ICommand ChooseCsvCommand { get; }
    public ICommand ImportCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand PreviousPageCommand { get; }
    public ICommand NextPageCommand { get; }
    public ICommand ExportExcelCommand { get; }
    public ICommand ExportXmlCommand { get; }

    public LanguageOption SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            _selectedLanguage = value;
            OnPropertyChanged();

            _localizationService.SetLanguage(value.Code);
            StatusText = Text("StatusLanguageChanged");
            UpdatePageText();
        }
    }

    public string? CsvPath
    {
        get => _csvPath;
        set
        {
            _csvPath = value;
            OnPropertyChanged();
        }
    }

    public DateTime? FromDate
    {
        get => _fromDate;
        set
        {
            _fromDate = value;
            OnPropertyChanged();
        }
    }

    public DateTime? ToDate
    {
        get => _toDate;
        set
        {
            _toDate = value;
            OnPropertyChanged();
        }
    }

    public string? FirstName
    {
        get => _firstName;
        set
        {
            _firstName = value;
            OnPropertyChanged();
        }
    }

    public string? LastName
    {
        get => _lastName;
        set
        {
            _lastName = value;
            OnPropertyChanged();
        }
    }

    public string? SurName
    {
        get => _surName;
        set
        {
            _surName = value;
            OnPropertyChanged();
        }
    }

    public string? City
    {
        get => _city;
        set
        {
            _city = value;
            OnPropertyChanged();
        }
    }

    public string? Country
    {
        get => _country;
        set
        {
            _country = value;
            OnPropertyChanged();
        }
    }

    public string StatusText
    {
        get => _statusText;
        private set
        {
            _statusText = value;
            OnPropertyChanged();
        }
    }

    public string PageText
    {
        get => _pageText;
        private set
        {
            _pageText = value;
            OnPropertyChanged();
        }
    }
    
    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            _isBusy = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsNotBusy));
        }
    }

    public bool IsNotBusy => !IsBusy;

    public void ShowInvalidDateWarning()
    {
        _dialogService.ShowWarning(Text("WarningInvalidDate"), Text("SearchTitle"));
    }

    private void ChooseCsv()
    {
        string? filePath = _dialogService.ChooseCsvFile(Text("CsvDialogFilter"), Text("CsvDialogTitle"));
        if (filePath == null)
        {
            return;
        }

        CsvPath = filePath;
        StatusText = Text("StatusCsvSelected");
        CommandManager.InvalidateRequerySuggested();
    }

    /// <summary>
    /// После подтверждения заменяет данные в базе данными из выбранного файла
    /// </summary>
    private async Task ImportAsync()
    {
        string? filePath = CsvPath;
        if (filePath == null)
        {
            return;
        }

        if (!_dialogService.Confirm(Text("ConfirmReplace"), Text("ImportTitle")))
        {
            return;
        }

        await ExecuteOperationAsync("StatusImporting", "ErrorImport", async () =>
        {
            CsvImportResult result = await Task.Run(() => _csvImportService.ImportAsync(filePath));
            FromDate = null;
            ToDate = null;
            FirstName = null;
            LastName = null;
            SurName = null;
            City = null;
            Country = null;

            await LoadRecordsAsync(new RecordFilter(), 1);

            string message = string.Format(Text("ImportSuccess"), result.ImportedCount, result.SkippedCount);
            _dialogService.ShowInformation(message, Text("ImportTitle"));
        });
    }

    private async Task SearchAsync()
    {
        RecordFilter filter = CreateFilter();
        if (!ValidateDateRange(filter, "SearchTitle"))
        {
            return;
        }

        await ExecuteOperationAsync("StatusSearching", "ErrorSearch", () => LoadRecordsAsync(filter, 1));
    }

    private async Task LoadPageAsync(int pageNumber)
    {
        await ExecuteOperationAsync("StatusSearching", "ErrorSearch", () => LoadRecordsAsync(_currentFilter, pageNumber));
    }

    /// <summary>
    /// Сохраняет записи в Excel
    /// </summary>
    private async Task ExportExcelAsync()
    {
        string? filePath = _dialogService.ChooseSaveFile(Text("ExcelDialogFilter"), Text("ExportExcelTitle"), "xlsx");
        if (filePath == null)
        {
            return;
        }
        
        string[] columnNames = GetColumnNames();
        string sheetName = Text("ExcelSheetName");
        RecordFilter filter = _currentFilter;

        await ExecuteOperationAsync("StatusExportingExcel", "ErrorExcel", async () =>
        {
            await Task.Run(() => _excelExportService.Export(
                filePath,
                _recordService.GetRecordsForExport(filter),
                columnNames,
                sheetName));
            StatusText = Text("StatusExcelCreated");
        });
    }

    /// <summary>
    /// Сохраняет записи в XML
    /// </summary>
    private async Task ExportXmlAsync()
    {
        string? filePath = _dialogService.ChooseSaveFile(Text("XmlDialogFilter"), Text("ExportXmlTitle"), "xml");
        if (filePath == null)
        {
            return;
        }

        RecordFilter filter = _currentFilter;

        await ExecuteOperationAsync("StatusExportingXml", "ErrorXml", async () =>
        {
            await Task.Run(() => _xmlExportService.Export(
                filePath,
                _recordService.GetRecordsForExport(filter)));
            StatusText = Text("StatusXmlCreated");
        });
    }

    /// <summary>
    /// Загружает страницу записей в таблицу и обновляет счётчики
    /// </summary>
    private async Task LoadRecordsAsync(RecordFilter filter, int pageNumber)
    {
        RecordSearchResult result = await _recordService.GetPageAsync(filter, pageNumber, PageSize);

        _currentFilter = filter;
        _currentPage = pageNumber;
        _totalRecords = result.TotalCount;

        Records.Clear();
        foreach (PersonRecord record in result.Records)
        {
            Records.Add(record);
        }

        if (_totalRecords == 0)
        {
            StatusText = Text("StatusNoRecords");
        }
        else
        {
            StatusText = string.Format(Text("StatusFound"), _totalRecords);
        }

        UpdatePageText();
    }

    /// <summary>
    /// Блокирует окно на время операции и показывает сообщение, если операция не удалась
    /// </summary>
    private async Task ExecuteOperationAsync(string statusKey, string errorKey, Func<Task> operation)
    {
        IsBusy = true;
        StatusText = Text(statusKey);

        try
        {
            await operation();
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
            StatusText = Text(errorKey);

            string message = Text(errorKey) + Environment.NewLine + Environment.NewLine + GetErrorDetails(exception);
            _dialogService.ShowError(message, Text("ErrorTitle"));
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Подбирает пользователю подсказку по типу ошибки
    /// </summary>
    private string GetErrorDetails(Exception exception)
    {
        switch (exception)
        {
            case FileNotFoundException:
                return Text("ErrorFileNotFound");
            case InvalidDataException:
                return Text("ErrorInvalidCsv");
            case UnauthorizedAccessException:
                return Text("ErrorAccessDenied");
            case IOException:
                return Text("ErrorFileIo");
            case DbUpdateException:
                return Text("ErrorDatabaseSave");
            case SqlException:
                return Text("ErrorDatabase");
            default:
                return Text("ErrorGeneric");
        }
    }

    private bool ValidateDateRange(RecordFilter filter, string titleKey)
    {
        if (filter.HasValidDateRange())
        {
            return true;
        }

        _dialogService.ShowWarning(Text("WarningDateRange"), Text(titleKey));
        return false;
    }

    private RecordFilter CreateFilter()
    {
        return new RecordFilter
        {
            FromDate = ToDateOnly(FromDate),
            ToDate = ToDateOnly(ToDate),
            FirstName = FirstName,
            LastName = LastName,
            SurName = SurName,
            City = City,
            Country = Country
        };
    }

    private void UpdatePageText()
    {
        int pageCount = GetPageCount();
        if (pageCount == 0)
        {
            PageText = Text("NoPages");
        }
        else
        {
            PageText = string.Format(Text("PageFormat"), _currentPage, pageCount);
        }
        
        CommandManager.InvalidateRequerySuggested();
    }
    
    private int GetPageCount()
    {
        return (int)Math.Ceiling((double)_totalRecords / PageSize);
    }

    private string[] GetColumnNames()
    {
        return new string[]
        {
            Text("ColumnDate"),
            Text("ColumnFirstName"),
            Text("ColumnLastName"),
            Text("ColumnSurName"),
            Text("ColumnCity"),
            Text("ColumnCountry")
        };
    }

    private string Text(string key)
    {
        return _localizationService.GetString(key);
    }

    private static DateOnly? ToDateOnly(DateTime? date)
    {
        if (date.HasValue)
        {
            return DateOnly.FromDateTime(date.Value);
        }

        return null;
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
