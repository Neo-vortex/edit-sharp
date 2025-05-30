using edit_sharp.Core.Interfaces;
using edit_sharp.Views.Dialogs;
using Terminal.Gui;

namespace edit_sharp.Plugins.BasicPlugins;

using edit_sharp.Views;
using System;

public class DialogPlugin : IEditorPlugin
{
    private IEditorHost _host;

    public string Name => "Simple Plugin";

    public void Initialize(IEditorHost host)
    {
        _host = host;
        var result = _host.CreateDialog<MyCustomDialog>("MyCustomDialog", 50, 50);
        Application.Run(result);
    }

    public void OnFileOpened(string filePath)
    {
        var result = _host.CreateDialog<MyCustomDialog2>("MyCustomDialog", 50, 50);
        Application.Run(result);
    }

    public void OnTextChanged(string? newText)
    {        
        var result = _host.CreateDialog<MyCustomDialog3>("MyCustomDialog", 50, 50);
        Application.Run(result);
    }
    
}
