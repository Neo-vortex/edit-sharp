using MoonSharp.Interpreter;
using edit_sharp.Core.Commands;
using edit_sharp.Core.Interfaces;
using edit_sharp.Views.Dialogs;
using Terminal.Gui;

namespace edit_sharp.Core.Lua;

// Lua Plugin Host - provides API to Lua scripts
public class LuaPluginHost
{
    private readonly IEditorHost _editorHost;
    private readonly Script _luaScript;
    private readonly string _pluginName;

    public string PluginName => _pluginName;

    public LuaPluginHost(IEditorHost editorHost, string pluginName)
    {
        _editorHost = editorHost;
        _pluginName = pluginName;
        _luaScript = new Script();
        
        // Register editor API for Lua
        RegisterEditorAPI();
    }

    private void RegisterEditorAPI()
    {
        // Create editor table with all API functions
        var editorTable = new Table(_luaScript);
        
        // Text operations
        editorTable["getText"] = (Func<string?>)(() => _editorHost.GetText());
        editorTable["setText"] = (Action<string>)(text => SetText(text));
        editorTable["getSelectedText"] = (Func<string?>)(() => GetSelectedText());
        editorTable["replaceSelectedText"] = (Action<string>)(text => ReplaceSelectedText(text));
        editorTable["insertText"] = (Action<string>)(text => InsertText(text));
        
        // File operations
        editorTable["getCurrentFile"] = (Func<string>)(() => _editorHost.CurrentFilePath ?? "");
        editorTable["getFileName"] = (Func<string>)(() => 
        {
            var path = _editorHost.CurrentFilePath;
            return string.IsNullOrEmpty(path) ? "Untitled" : Path.GetFileName(path);
        });
        
        // UI operations
        editorTable["showMessage"] = (Action<string, string>)((title, msg) => 
            MessageBox.Query(50, 7, title, msg, "OK"));
        editorTable["setStatus"] = (Action<string>)(msg => _editorHost.SetStatusMessage(msg));
        
        // Input dialogs
        editorTable["getInput"] = (Func<string, string, string, string?>)GetInput;
        editorTable["getMultiLineInput"] = (Func<string, string, string, string?>)GetMultiLineInput;
        editorTable["confirm"] = (Func<string, string, bool>)Confirm;
        
        // Command registration
        editorTable["registerCommand"] = (Action<string, string, string, DynValue>)RegisterCommand;
        
        _luaScript.Globals["editor"] = editorTable;
    }
    private string? GetInput(string title, string prompt, string defaultValue)
    {
        var dialog = new InputDialog(title, prompt, defaultValue);
        Application.Run(dialog);
        return dialog.Cancelled ? null : dialog.InputValue;
    }

    private string? GetMultiLineInput(string title, string prompt, string defaultValue)
    {
        var dialog = new MultiLineInputDialog(title, prompt, defaultValue);
        Application.Run(dialog);
        return dialog.Cancelled ? null : dialog.InputValue;
    }

    private bool Confirm(string title, string message)
    {
        var dialog = new ConfirmDialog(title, message);
        Application.Run(dialog);
        return dialog.Confirmed;
    }

    private void SetText(string text)
    {
        var textView = _editorHost.GetTextView();
        textView.Text = text;
    }

    private string? GetSelectedText()
    {
        var textView = _editorHost.GetTextView();
        return textView.SelectedText?.ToString();
    }

    private void ReplaceSelectedText(string newText)
    {
        var textView = _editorHost.GetTextView();
        var selectedText = GetSelectedText();
        
        if (!string.IsNullOrEmpty(selectedText))
        {
            var fullText = _editorHost.GetText() ?? "";
            var selection = textView.SelectedText?.ToString();
            
            if (!string.IsNullOrEmpty(selection))
            {
                // Find the selection in the text
                var selectionStart = fullText.IndexOf(selection, StringComparison.Ordinal);
                
                if (selectionStart >= 0)
                {
                    var before = fullText.Substring(0, selectionStart);
                    var after = fullText.Substring(selectionStart + selection.Length);
                    SetText(before + newText + after);
                }
            }
        }
        else
        {
            // If no selection, just insert at cursor
            InsertText(newText);
        }
    }

    private void InsertText(string text)
    {
        var textView = _editorHost.GetTextView();
        var cursorPos = textView.CursorPosition;
        var currentText = textView.Text.ToString() ?? "";
        
        // Split into lines
        var lines = currentText.Split('\n').ToList();
        
        // Ensure we have enough lines
        while (lines.Count <= cursorPos.Y)
        {
            lines.Add("");
        }
        
        if (cursorPos.Y < lines.Count)
        {
            var line = lines[cursorPos.Y];
            var col = Math.Min(cursorPos.X, line.Length);
            lines[cursorPos.Y] = line.Insert(col, text);
            SetText(string.Join("\n", lines));
        }
    }

    private void RegisterCommand(string id, string name, string category, DynValue luaFunction)
    {
        var command = new EditorCommand(id, name, category, () =>
        {
            try
            {
                _luaScript.Call(luaFunction);
            }
            catch (ScriptRuntimeException ex)
            {
                MessageBox.ErrorQuery(50, 7, "Lua Error", 
                    $"Error executing command '{name}':\n\n{ex.DecoratedMessage}", "OK");
            }
            catch (Exception ex)
            {
                MessageBox.ErrorQuery(50, 7, "Command Error", 
                    $"Error executing command '{name}':\n\n{ex.Message}", "OK");
            }
        })
        {
            Description = $"Loaded from {_pluginName}"
        };

        CommandRegistry.Instance.Register(command);
    }

    public void ExecuteScript(string luaCode)
    {
        try
        {
            _luaScript.DoString(luaCode);
        }
        catch (SyntaxErrorException ex)
        {
            throw new Exception($"Lua syntax error in {_pluginName}: {ex.DecoratedMessage}", ex);
        }
        catch (ScriptRuntimeException ex)
        {
            throw new Exception($"Lua runtime error in {_pluginName}: {ex.DecoratedMessage}", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error loading {_pluginName}: {ex.Message}", ex);
        }
    }

    public void LoadPluginFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Plugin file not found: {filePath}");

        var code = File.ReadAllText(filePath);
        ExecuteScript(code);
    }
}

// Lua Plugin Manager - manages loading/unloading of Lua plugins
public class LuaPluginManager
{
    private readonly IEditorHost _editorHost;
    private readonly List<LuaPluginHost> _loadedPlugins = new();
    private readonly string _pluginsDirectory;

    public int LoadedPluginCount => _loadedPlugins.Count;
    public IReadOnlyList<LuaPluginHost> LoadedPlugins => _loadedPlugins.AsReadOnly();

    public LuaPluginManager(IEditorHost editorHost, string pluginsDirectory)
    {
        _editorHost = editorHost;
        _pluginsDirectory = pluginsDirectory;
    }

    public void LoadAllPlugins()
    {
        if (!Directory.Exists(_pluginsDirectory))
        {
            Directory.CreateDirectory(_pluginsDirectory);
            _editorHost.SetStatusMessage($"Created plugins directory: {_pluginsDirectory}");
            return;
        }

        var luaFiles = Directory.GetFiles(_pluginsDirectory, "*.lua", SearchOption.AllDirectories);
        
        if (luaFiles.Length == 0)
        {
            _editorHost.SetStatusMessage("No Lua plugins found in plugins directory");
            return;
        }

        var successCount = 0;
        var failCount = 0;

        foreach (var file in luaFiles)
        {
            try
            {
                LoadPlugin(file);
                successCount++;
            }
            catch (Exception ex)
            {
                failCount++;
                var fileName = Path.GetFileName(file);
                
                // Log error but continue loading other plugins
                Console.WriteLine($"Failed to load plugin {fileName}: {ex.Message}");
                
                // Optionally show error dialog (comment out if too noisy)
                // MessageBox.ErrorQuery(60, 10, "Plugin Load Error", 
                //     $"Failed to load {fileName}:\n\n{ex.Message}", "OK");
            }
        }

        var statusMessage = successCount > 0 
            ? $"Loaded {successCount} Lua plugin(s)" 
            : "No plugins loaded";

        if (failCount > 0)
        {
            statusMessage += $" ({failCount} failed)";
        }

        _editorHost.SetStatusMessage(statusMessage);
    }

    public void LoadPlugin(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        var plugin = new LuaPluginHost(_editorHost, fileName);
        plugin.LoadPluginFile(filePath);
        _loadedPlugins.Add(plugin);
    }

    public void ReloadPlugins()
    {
        UnloadAllPlugins();
        LoadAllPlugins();
    }

    public void UnloadAllPlugins()
    {
        _loadedPlugins.Clear();
        
        // Clear all registered commands (both built-in and plugin commands will be cleared)
        // You may want to keep track of which commands are built-in vs plugin commands
        // For now, we'll clear everything and let the app re-register built-ins
        CommandRegistry.Instance.Clear();
        
        _editorHost.SetStatusMessage("All plugins unloaded");
    }

    public void UnloadPlugin(string pluginName)
    {
        var plugin = _loadedPlugins.FirstOrDefault(p => p.PluginName == pluginName);
        if (plugin != null)
        {
            _loadedPlugins.Remove(plugin);
            _editorHost.SetStatusMessage($"Unloaded plugin: {pluginName}");
        }
    }

    public List<string> GetLoadedPluginNames()
    {
        return _loadedPlugins.Select(p => p.PluginName).ToList();
    }
}