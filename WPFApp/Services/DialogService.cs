using System.Windows;
using Microsoft.Win32;

namespace WPFApp.Services;

/// <summary>
/// Показ диалоговых окон
/// </summary>
public class DialogService : IDialogService
{
    /// <summary>
    /// Возвращает путь к CSV или null при отмене
    /// </summary>
    public string? ChooseCsvFile(string filter, string title)
    {
        OpenFileDialog dialog = new OpenFileDialog
        {
            Filter = filter,
            Title = title
        };

        if (dialog.ShowDialog() == true)
        {
            return dialog.FileName;
        }

        return null;
    }

    /// <summary>
    /// Возвращает путь для сохранения или null при отмене
    /// </summary>
    public string? ChooseSaveFile(string filter, string title, string extension)
    {
        SaveFileDialog dialog = new SaveFileDialog
        {
            Filter = filter,
            Title = title,
            DefaultExt = extension,
            AddExtension = true,
            OverwritePrompt = true
        };

        if (dialog.ShowDialog() == true)
        {
            return dialog.FileName;
        }

        return null;
    }

    public bool Confirm(string message, string title)
    {
        MessageBoxResult result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        return result == MessageBoxResult.Yes;
    }

    public void ShowInformation(string message, string title)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void ShowWarning(string message, string title)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    public void ShowError(string message, string title)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
