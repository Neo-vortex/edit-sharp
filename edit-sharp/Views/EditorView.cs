using edit_sharp.Core.Interfaces;
using edit_sharp.Core.Services;
using NStack;
using Terminal.Gui;

namespace edit_sharp.Views;

public class EditorView : ITextEditor, IEditorHost
{
    private bool _hasUnsavedChanges;
    private LineNumberView? _lineNumberView;
    private readonly Window _parentWindow;
    private bool _showLineNumbers = true;
    private bool _showStatusBar = true;
    private readonly Label _statusBar;
    private bool _wordWrap = true;
    private readonly List<IEditorPlugin> _plugins;
    private string _lineEnding = "Unknown";
    private string _encoding = "Unknown";
    private string _lineCount = "Unknown";
    private string? _charCount = "Unknown";

    public EditorView(Window parentWindow)
    {
        _parentWindow = parentWindow ?? throw new ArgumentNullException(nameof(parentWindow));

        _plugins = PluginLoader.LoadPlugins(Environment.CurrentDirectory, this);

        TextView = new TextView
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill() - 2,
            WordWrap = _wordWrap
        };

        TextView.TextChanged += () =>
        {
            var content = TextView.Text.ToString();
            _lineCount = TextView.Lines.ToString();
            _charCount = content?.Length.ToString();
            _hasUnsavedChanges = true;
            TextChanged?.Invoke();
            UpdateStatus($"Editing {Path.GetFileName(FilePath)} - Unsaved changes");
            foreach (var plugin in _plugins) plugin.OnTextChanged(TextView.Text.ToString());
        };

        _parentWindow.Add(TextView);

        _statusBar = new Label
        {
            X = 0,
            Y = Pos.Bottom(TextView),
            Width = Dim.Fill(),
            Height = 1,
            Text = "Ready",
            Visible = _showStatusBar,
            ColorScheme = Colors.Menu
        };

        _parentWindow.Add(_statusBar);
    }

    public string? Text
    {
        get => TextView.Text.ToString();
        set
        {
            TextView.Text = value;
            _hasUnsavedChanges = false;
        }
    }

    public string? FilePath { get; set; } = string.Empty;

    public bool HasUnsavedChanges => _hasUnsavedChanges || TextView.IsDirty;

    public TextView TextView { get; }

    public MenuBar MenuBar { get; set; }

    public void NewFile()
    {
        if (!CheckUnsavedChanges()) return;

        Text = string.Empty;
        FilePath = string.Empty;
        _hasUnsavedChanges = false;
        UpdateStatus("New file created");
    }

    public void OpenFile(string? path = null)
    {
        if (!CheckUnsavedChanges()) return;

        if (string.IsNullOrEmpty(path))
        {
            var dlg = CreateDialog<OpenDialog>("Open File", 60, 20);
            Application.Run(dlg);
            if (dlg.FilePath != FilePath)
                path = dlg.FilePath.ToString();
            else
                return;
        }

        try
        {
            if (File.Exists(path))
            {
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
                using var reader = new StreamReader(stream, detectEncodingFromByteOrderMarks: true);
                var content = reader.ReadToEnd();
                Text = content;
                _encoding = reader.CurrentEncoding.EncodingName;
                _lineCount = TextView.Lines.ToString();
                _charCount = content.Length.ToString();
                _lineEnding = DetectLineEnding(content);
                FilePath = path;
                _hasUnsavedChanges = false;
                UpdateStatus($"Opened: {Path.GetFileName(path)}");
                FileOpened?.Invoke();
                foreach (var plugin in _plugins) plugin.OnFileOpened(path);
            }
            else
            {
                ShowError("File not found.");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Error opening file: {ex.Message}");
        }
    }
    private static string DetectLineEnding(string content)
    {
        var crlf = content.Split("\r\n").Length - 1;
        var lf = content.Split("\n").Length - 1;
        var cr = content.Split("\r").Length - 1;

        if (crlf > 0 && crlf >= lf && crlf >= cr)
            return "CRLF";
        if (lf > 0 && lf >= cr)
            return "LF";
        return cr > 0 ? "CR" : "Unknown";
    }

    public void SaveFile()
    {
        if (string.IsNullOrEmpty(FilePath))
        {
            SaveFileAs();
            return;
        }

        var choice = MessageBox.Query(50, 7, "Confirm Save", $"Do you want to save '{Path.GetFileName(FilePath)}'?",
            "Yes", "No");
        if (choice != 0) return;

        try
        {
            File.WriteAllText(FilePath, Text);
            _hasUnsavedChanges = false;
            UpdateStatus($"Saved: {Path.GetFileName(FilePath)}");
            FileSaved?.Invoke();
        }
        catch (Exception ex)
        {
            ShowError($"Error saving file: {ex.Message}");
        }
    }

    public void SaveFileAs()
    {
        var dlg = CreateDialog<SaveDialog>("Save File As", 60, 20);
        Application.Run(dlg);
        if (dlg.FileName == FilePath) return;
        FilePath = dlg.FilePath.ToString();
        SaveFile();
    }

    public void CloseFile()
    {
        if (HasUnsavedChanges)
        {
            var choice = MessageBox.Query(50, 7, "Unsaved Changes", "Do you want to save changes before closing?",
                "Save", "Discard", "Cancel");
            switch (choice)
            {
                case 0:
                    SaveFile();
                    break;
                case 1:
                    break;
                default:
                    return;
            }
        }

        Text = string.Empty;
        FilePath = string.Empty;
        _hasUnsavedChanges = false;
        UpdateStatus("File closed");
    }

    public bool CheckUnsavedChanges()
    {
        if (!HasUnsavedChanges) return true;

        var n = MessageBox.Query(50, 7, "Unsaved Changes", "You have unsaved changes. Save now?", "Yes", "No",
            "Cancel");

        switch (n)
        {
            case 0:
                SaveFile();
                return true;
            case 1:
                return true;
            default:
                return false;
        }
    }

    public void ToggleLineNumbers()
    {
        _showLineNumbers = !_showLineNumbers;

        if (_showLineNumbers)
        {
            _lineNumberView = new LineNumberView(TextView)
            {
                X = 0,
                Y = 0,
                Height = Dim.Fill()
            };

            _parentWindow.Add(_lineNumberView);
            TextView.X = Pos.Right(_lineNumberView);
        }
        else
        {
            if (_lineNumberView != null)
            {
                _parentWindow.Remove(_lineNumberView);
                _lineNumberView = null;
            }

            TextView.X = 0;
        }

        _parentWindow.SetNeedsDisplay();
    }

    public void ToggleStatusBar()
    {
        _showStatusBar = !_showStatusBar;
        _statusBar.Visible = _showStatusBar;
        _parentWindow.SetNeedsDisplay();
    }

    public void ToggleWordWrap()
    {
        _wordWrap = !_wordWrap;
        TextView.WordWrap = _wordWrap;
        _parentWindow.SetNeedsDisplay();
    }

    public void ShowMessage(string title, string message)
    {
        MessageBox.Query(50, 7, title, message, "OK");
    }

    public void ShowError(string message)
    {
        MessageBox.ErrorQuery(50, 7, "Error", message, "OK");
    }

    public void UpdateStatus(string message)
    {
        if (!_showStatusBar) return;
        var lineCount = TextView.Lines;
        var charCount = TextView.Text.Length;

        _statusBar.Text = $"{message} [Lines: {lineCount}, Chars: {charCount}, {_encoding}, {_lineEnding}]";
    }

    public T CreateDialog<T>(string title, int width, int height) where T : Dialog, new()
    {
        var dlg = new T
        {
            Title = title,
            Width = width,
            Height = height,
            X = Pos.Center(),
            Y = Pos.Center()
        };
        return dlg;
    }

    public event Action TextChanged;
    public event Action FileOpened;
    public event Action FileSaved;
    public string CurrentFilePath { get; }

    public string? GetText()
    {
        return TextView.Text.ToString();
    }

    public void SetStatusMessage(string message)
    {
        UpdateStatus(message);
    }

    public void AddMenuItem(string menuPath, Action action)
    {

    }
}
