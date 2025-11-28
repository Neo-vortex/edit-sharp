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
    private string? _detectedLineEnding = null; // Line ending from current editing session
    private string _encoding = "UTF-8"; // Default encoding
    private bool _programmaticChange = false; // Flag to track programmatic changes
    private string _lastText = string.Empty; // Track last text state

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

        // Use KeyPress event instead of TextChanged for keyboard input detection
        TextView.KeyPress += OnKeyPress;

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


    private void OnKeyPress(View.KeyEventEventArgs args)
    {
        // Check if this is an actual text modification key
        var key = args.KeyEvent.Key;
        
        // Filter out navigation and non-text keys
        if (IsNavigationKey(key) || IsModifierOnlyKey(key))
        {
            return;
        }

        // Schedule the text change notification for after the key is processed
        Application.MainLoop.AddTimeout(TimeSpan.FromMilliseconds(10), (_) =>
        {
            var currentText = TextView.Text.ToString();
            if (currentText != _lastText && !_programmaticChange)
            {
                _lastText = currentText;
                OnUserTextChanged();
            }
            return false; // Don't repeat
        });
    }

    private bool IsNavigationKey(Key key)
    {
        var keyWithoutMods = key & ~(Key.CtrlMask | Key.ShiftMask | Key.AltMask);
        
        return keyWithoutMods switch
        {
            Key.CursorUp or Key.CursorDown or Key.CursorLeft or Key.CursorRight or
            Key.Home or Key.End or Key.PageUp or Key.PageDown or
            Key.F1 or Key.F2 or Key.F3 or Key.F4 or Key.F5 or
            Key.F6 or Key.F7 or Key.F8 or Key.F9 or Key.F10 or
            Key.F11 or Key.F12 or Key.Esc or Key.Tab => true,
            _ => false
        };
    }

    private bool IsModifierOnlyKey(Key key)
    {
        // Check if only modifier keys are pressed (Ctrl, Alt, Shift alone)
        return key == Key.CtrlMask || key == Key.AltMask || key == Key.ShiftMask;
    }

    private void OnUserTextChanged()
    {
        var content = TextView.Text.ToString();
        _hasUnsavedChanges = true;
        
        // Detect line ending from user input
        DetectLineEndingFromInput(content);
        
        TextChanged?.Invoke();
        UpdateWindowTitle();
        UpdateStatus();
        
        // Notify plugins
        foreach (var plugin in _plugins)
        {
            plugin.OnTextChanged(content);
        }
    }

    private void DetectLineEndingFromInput(string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            _detectedLineEnding = null;
            return;
        }

        // Only detect if we haven't detected yet or if content has line breaks
        for (int i = 0; i < content.Length; i++)
        {
            if (content[i] == '\r')
            {
                if (i + 1 < content.Length && content[i + 1] == '\n')
                {
                    _detectedLineEnding = "CRLF";
                    return;
                }
                else
                {
                    _detectedLineEnding = "CR";
                    return;
                }
            }
            else if (content[i] == '\n')
            {
                _detectedLineEnding = "LF";
                return;
            }
        }
    }

    public string? Text
    {
        get => TextView.Text.ToString();
        set
        {
            _programmaticChange = true;
            TextView.Text = value;
            _lastText = value ?? string.Empty;
            _hasUnsavedChanges = false;
            _programmaticChange = false;
            UpdateWindowTitle();
            UpdateStatus(); // Update status after setting text
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
        _detectedLineEnding = null;
        _encoding = "UTF-8";
        _hasUnsavedChanges = false;
        UpdateWindowTitle();
        UpdateStatus();
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
                _detectedLineEnding = DetectLineEndingFromFile(content);
                FilePath = path;
                _hasUnsavedChanges = false;
                UpdateWindowTitle();
                UpdateStatus();
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

    private static string DetectLineEndingFromFile(string content)
    {
        if (string.IsNullOrEmpty(content))
            return "Unknown";

        var crlfCount = 0;
        var lfCount = 0;
        var crCount = 0;

        for (int i = 0; i < content.Length; i++)
        {
            if (content[i] == '\r')
            {
                if (i + 1 < content.Length && content[i + 1] == '\n')
                {
                    crlfCount++;
                    i++; // Skip the \n
                }
                else
                {
                    crCount++;
                }
            }
            else if (content[i] == '\n')
            {
                lfCount++;
            }
        }

        if (crlfCount > 0 && crlfCount >= lfCount && crlfCount >= crCount)
            return "CRLF";
        if (lfCount > 0 && lfCount >= crCount)
            return "LF";
        return crCount > 0 ? "CR" : "Unknown";
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
            UpdateWindowTitle();
            UpdateStatus();
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
        _detectedLineEnding = null;
        _hasUnsavedChanges = false;
        UpdateWindowTitle();
        UpdateStatus();
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

    public void UpdateStatus(string? customMessage = null)
    {
        if (!_showStatusBar) return;

        var lineCount = TextView.Lines;
        var charCount = TextView.Text.Length;
        var lineEnding = _detectedLineEnding ?? "---";
        
        // Build status parts
        var parts = new List<string>();
        
        // Custom message or default status
        if (!string.IsNullOrEmpty(customMessage))
        {
            parts.Add(customMessage);
        }
        else if (!string.IsNullOrEmpty(FilePath))
        {
            parts.Add(Path.GetFileName(FilePath));
        }
        else
        {
            parts.Add("Untitled");
        }
        
        // Add stats
        parts.Add($"Lines: {lineCount}");
        parts.Add($"Chars: {charCount}");
        parts.Add($"Encoding: {_encoding}");
        parts.Add($"Line Ending: {lineEnding}");
        
        _statusBar.Text = string.Join(" | ", parts);
    }

    private void UpdateWindowTitle()
    {
        var fileName = string.IsNullOrEmpty(FilePath) ? "Untitled" : Path.GetFileName(FilePath);
        var modifiedIndicator = _hasUnsavedChanges ? "● " : "";
        _parentWindow.Title = $"{modifiedIndicator}{fileName} - Edit Sharp";
    }

    public void SetStatusMessage(string message)
    {
        UpdateStatus(message);
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
    
    public string CurrentFilePath => FilePath ?? string.Empty;

    public string? GetText()
    {
        return TextView.Text.ToString();
    }

    public void SetText(string text)
    {
        Text = text;
    }

    public TextView GetTextView()
    {
        return TextView;
    }



    public void AddMenuItem(string menuPath, Action action)
    {
        // This would require parsing menuPath (e.g., "Tools/Transform/Upper Case")
        // and adding items to the MenuBar dynamically
    }
}