using System.Windows.Controls;
using MahApps.Metro.Controls;
using WPFApp.ViewModels;

namespace WPFApp;

/// <summary>
/// Главное окно. Вся логика находится в MainWindowViewModel.
/// </summary>
public partial class MainWindow : MetroWindow
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void DatePicker_DateValidationError(object sender, DatePickerDateValidationErrorEventArgs e)
    {
        e.ThrowException = false;
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.ShowInvalidDateWarning();
        }
    }
}
