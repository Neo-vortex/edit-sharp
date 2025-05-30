namespace edit_sharp.Core.Interfaces;

public interface IEditorPlugin
{
    string Name { get; }
    void Initialize(IEditorHost host); 
    void OnFileOpened(string filePath);
    void OnTextChanged(string? newText);

}
