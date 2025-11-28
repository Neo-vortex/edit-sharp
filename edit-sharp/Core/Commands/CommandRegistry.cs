namespace edit_sharp.Core.Commands;

// Command Registry - manages all available commands
public class CommandRegistry
{
    private readonly Dictionary<string, EditorCommand> _commands = new();
    private static CommandRegistry? _instance;

    public static CommandRegistry Instance => _instance ??= new CommandRegistry();

    public void Register(EditorCommand command)
    {
        _commands[command.Id] = command;
    }

    public void Unregister(string commandId)
    {
        _commands.Remove(commandId);
    }

    public EditorCommand? GetCommand(string commandId)
    {
        return _commands.TryGetValue(commandId, out var cmd) ? cmd : null;
    }

    public List<EditorCommand> GetAllCommands()
    {
        return _commands.Values.ToList();
    }

    public List<EditorCommand> SearchCommands(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return GetAllCommands();

        query = query.ToLower();
        return _commands.Values
            .Where(c => c.Name.ToLower().Contains(query) || 
                        c.Category.ToLower().Contains(query) ||
                        c.Description?.ToLower().Contains(query) == true)
            .ToList();
    }

    public void Clear()
    {
        _commands.Clear();
    }
}