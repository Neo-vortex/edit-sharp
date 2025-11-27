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
        
    }

    public void OnFileOpened(string filePath)
    {
        
    }

    public void OnTextChanged(string? newText)
    {        
       
    }
    
}
