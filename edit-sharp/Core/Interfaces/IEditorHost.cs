using Terminal.Gui;


namespace edit_sharp.Core.Interfaces;

public interface IEditorHost
{
    // File operations
    string CurrentFilePath { get; }
    
    // Text operations
    string? GetText();
    void SetText(string text);
    
    // TextView access for advanced manipulation
    TextView GetTextView();
    
    // UI operations
    void SetStatusMessage(string message);
    T CreateDialog<T>(string title, int width, int height) where T : Dialog, new();
    
    // Menu operations
    void AddMenuItem(string menuPath, Action action);
}