namespace WPFApp.Services;

/// <summary>
/// Интерфейс диалогов
/// </summary>
public interface IDialogService
{
    string? ChooseCsvFile(string filter, string title);
    string? ChooseSaveFile(string filter, string title, string extension);
    bool Confirm(string message, string title);
    void ShowInformation(string message, string title);
    void ShowWarning(string message, string title);
    void ShowError(string message, string title);
}
