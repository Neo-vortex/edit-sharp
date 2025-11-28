using Terminal.Gui;

namespace edit_sharp.Core.Commands;

public class EditorCommand
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Category { get; set; }
    public string? Description { get; set; }
    public Key? KeyBinding { get; set; }
    public Action Execute { get; set; }
    public Func<bool>? CanExecute { get; set; }

    public EditorCommand(string id, string name, string category, Action execute)
    {
        Id = id;
        Name = name;
        Category = category;
        Execute = execute;
    }

    public bool IsEnabled()
    {
        return CanExecute?.Invoke() ?? true;
    }

    public string DisplayName => $"{Category}: {Name}";
}