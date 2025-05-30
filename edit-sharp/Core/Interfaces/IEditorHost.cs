using Terminal.Gui;

namespace edit_sharp.Core.Interfaces;

public interface IEditorHost
{
    string CurrentFilePath { get; }
    string? GetText();
    void SetStatusMessage(string message);
    void AddMenuItem(string menuPath, Action action);
    T CreateDialog<T>(string title, int width, int height) where T : Dialog, new();
}