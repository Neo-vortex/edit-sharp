using Terminal.Gui;

namespace edit_sharp.Core.Interfaces
{
    public interface ITextEditor
    {
        string? Text { get; set; }
        string? FilePath { get; set; }
        bool HasUnsavedChanges { get; }
        TextView TextView { get; }
        MenuBar MenuBar { get; set; }

        void NewFile();
        void OpenFile(string? path);
        void SaveFile();
        void SaveFileAs();
        void CloseFile();
        bool CheckUnsavedChanges();
        
        void ToggleLineNumbers();
        void ToggleStatusBar();
        void ToggleWordWrap();
        
        void ShowMessage(string title, string message);
        void ShowError(string message);
        void UpdateStatus(string message);
        T CreateDialog<T>(string title, int width, int height) where T : Dialog, new();
        
        event Action TextChanged;
        event Action FileOpened;
        event Action FileSaved;
    }
}