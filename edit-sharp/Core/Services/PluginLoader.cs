using System.Reflection;
using edit_sharp.Core.Interfaces;

namespace edit_sharp.Core.Services;

public static class PluginLoader
{
    public static List<IEditorPlugin> LoadPlugins(string pluginsPath, IEditorHost host)
    {
        var plugins = new List<IEditorPlugin>();
        var dllFiles = Directory.GetFiles(pluginsPath, "*.dll");

        foreach (var dll in dllFiles)
        {
            try
            {
                var asm = Assembly.LoadFrom(dll);
                var types = asm.GetTypes().Where(t => typeof(IEditorPlugin).IsAssignableFrom(t) && !t.IsAbstract);
                foreach (var type in types)
                {
                    if (Activator.CreateInstance(type) is not IEditorPlugin plugin) continue;
                    plugin.Initialize(host);
                    plugins.Add(plugin);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load plugin from {dll}: {ex.Message}");
            }
        }

        return plugins;
    }
}