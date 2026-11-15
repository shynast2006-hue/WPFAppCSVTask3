using System.Diagnostics;
using System.Windows;
using WPFApp.Data;
using WPFApp.Services;
using WPFApp.ViewModels;

namespace WPFApp;

/// <summary>
/// Создаёт базу данных, если её ещё нет, и открывает главное окно.
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        LocalizationService localizationService = new LocalizationService();

        try
        {
            string connectionString = AppConfiguration.GetConnectionString();

            using (AppDbContext context = new AppDbContext(connectionString))
            {
                context.Database.EnsureCreated();
            }

            MainWindowViewModel viewModel = new MainWindowViewModel(
                new CsvImportService(connectionString),
                new RecordService(connectionString),
                new ExcelExportService(),
                new XmlExportService(),
                new DialogService(),
                localizationService);

            MainWindow = new MainWindow(viewModel);
            MainWindow.Show();
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
            MessageBox.Show(
                localizationService.GetString("ErrorStartup"),
                localizationService.GetString("ErrorTitle"),
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown();
        }
    }
}
