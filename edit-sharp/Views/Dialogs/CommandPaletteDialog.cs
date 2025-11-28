using edit_sharp.Core.Commands;
using Terminal.Gui;

namespace edit_sharp.Views.Dialogs;

public class CommandPaletteDialog : Dialog
{
    private readonly TextField _searchField;
    private readonly ListView _commandList;
    private List<EditorCommand> _filteredCommands;
    private readonly CommandRegistry _registry;

    public EditorCommand? SelectedCommand { get; private set; }

    public CommandPaletteDialog() : base("Command Palette", 80, 20)
    {
        _registry = CommandRegistry.Instance;
        _filteredCommands = _registry.GetAllCommands();

        // Search field
        var searchLabel = new Label("Search:")
        {
            X = 1,
            Y = 1
        };
        Add(searchLabel);

        _searchField = new TextField("")
        {
            X = Pos.Right(searchLabel) + 1,
            Y = 1,
            Width = Dim.Fill() - 1
        };

        _searchField.TextChanged += (oldValue) =>
        {
            FilterCommands(_searchField.Text.ToString());
        };

        Add(_searchField);

        // Command list
        _commandList = new ListView(_filteredCommands.Select(c => c.DisplayName).ToList())
        {
            X = 1,
            Y = 3,
            Width = Dim.Fill() - 1,
            Height = Dim.Fill() - 2,
            AllowsMarking = false
        };

        _commandList.OpenSelectedItem += (args) =>
        {
            ExecuteSelectedCommand();
        };

        Add(_commandList);

        // Buttons
        var executeButton = new Button("Execute")
        {
            X = Pos.Center() - 10,
            Y = Pos.Bottom(this) - 4,
            IsDefault = true
        };
        executeButton.Clicked += ExecuteSelectedCommand;

        var cancelButton = new Button("Cancel")
        {
            X = Pos.Center() + 2,
            Y = Pos.Bottom(this) - 4
        };
        cancelButton.Clicked += () => Application.RequestStop();

        Add(executeButton);
        Add(cancelButton);

        // Set focus to search field
        _searchField.SetFocus();

        // Add keyboard shortcuts
        KeyPress += (e) =>
        {
            if (e.KeyEvent.Key == Key.Esc)
            {
                Application.RequestStop();
                e.Handled = true;
            }
            else if (e.KeyEvent.Key == Key.Enter && _searchField.HasFocus)
            {
                ExecuteSelectedCommand();
                e.Handled = true;
            }
            else if (e.KeyEvent.Key == Key.CursorDown && _searchField.HasFocus)
            {
                _commandList.SetFocus();
                e.Handled = true;
            }
        };
    }

    private void FilterCommands(string? query)
    {
        _filteredCommands = _registry.SearchCommands(query ?? "");
        _commandList.SetSource(_filteredCommands.Select(c => c.DisplayName).ToList());
        _commandList.SelectedItem = 0;
    }

    private void ExecuteSelectedCommand()
    {
        if (_commandList.SelectedItem >= 0 && _commandList.SelectedItem < _filteredCommands.Count)
        {
            SelectedCommand = _filteredCommands[_commandList.SelectedItem];
            
            if (SelectedCommand.IsEnabled())
            {
                Application.RequestStop();
                // Execute after dialog closes
                Application.MainLoop.AddTimeout(TimeSpan.FromMilliseconds(100), (_) =>
                {
                    try
                    {
                        SelectedCommand.Execute();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.ErrorQuery(50, 7, "Command Error", 
                            $"Failed to execute command: {ex.Message}", "OK");
                    }
                    return false;
                });
            }
            else
            {
                MessageBox.ErrorQuery(50, 7, "Command Disabled", 
                    "This command is currently disabled.", "OK");
            }
        }
    }
}