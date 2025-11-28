using System;
using Terminal.Gui;
using edit_sharp.Views;
using edit_sharp.Core.Commands;
using edit_sharp.Core.Lua;
using edit_sharp.Views.Dialogs;

public static class Program
{
    public static void Main()
    {
        Application.Init();
        var top = Application.Top!;

        // -----------------------------------------------------
        // UI Color Schemes
        // -----------------------------------------------------
        var menuColors = new ColorScheme
        {
            Normal = Application.Driver.MakeAttribute(Color.Gray, Color.Blue),
            Focus = Application.Driver.MakeAttribute(Color.BrightGreen, Color.Gray),
            Disabled = Application.Driver.MakeAttribute(Color.DarkGray, Color.Blue),
            HotNormal = Application.Driver.MakeAttribute(Color.BrightBlue, Color.Blue),
            HotFocus = Application.Driver.MakeAttribute(Color.Green, Color.Gray)
        };

        var headerFooterColors = new ColorScheme
        {
            Normal = Application.Driver.MakeAttribute(Color.White, Color.Black),
            Focus = Application.Driver.MakeAttribute(Color.BrightYellow, Color.Black)
        };

        var editorColors = new ColorScheme
        {
            Normal = Application.Driver.MakeAttribute(Color.White, Color.Black),
            Focus = Application.Driver.MakeAttribute(Color.BrightYellow, Color.Black),
            Disabled = Application.Driver.MakeAttribute(Color.Gray, Color.Black)
        };

        // -----------------------------------------------------
        // Main Window & Editor
        // -----------------------------------------------------
        var window = new Window("Edit Sharp")
        {
            X = 0,
            Y = 1,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            ColorScheme = headerFooterColors
        };

        var editor = new EditorView(window)
        {
            TextView = { ColorScheme = editorColors },
            MenuBar = new MenuBar([])
        };

        top.Add(window);

        // -----------------------------------------------------
        // Plugins (FULL version only)
        // -----------------------------------------------------
#if PLUGIN_SUPPORT
        LoadLuaPlugins(editor);
#else
    editor.SetStatusMessage("Plugin system disabled in this build.");
#endif

        // -----------------------------------------------------
        // Built-in Commands
        // -----------------------------------------------------
        RegisterBuiltIns(editor);

        // -----------------------------------------------------
        // Menu
        // -----------------------------------------------------
        var menu = BuildMenu(editor);
        menu.ColorScheme = menuColors;

        editor.MenuBar = menu;
        top.Add(menu);

        // -----------------------------------------------------
        // Run App
        // -----------------------------------------------------
        Application.Run(top);
    }


    // =========================================================
    // Plugin Loader
    // =========================================================
#if PLUGIN_SUPPORT
    private static void LoadLuaPlugins(EditorView editor)
    {
        try
        {
            var path = Path.Combine(
                AppContext.BaseDirectory,
                "Plugins",
                "Lua"
            );

            var manager = new LuaPluginManager(editor, path);
            manager.LoadAllPlugins();

            editor.SetStatusMessage("Plugin system is ready");
        }
        catch (Exception ex)
        {
            MessageBox.ErrorQuery(
                50, 7,
                "Plugin Error",
                $"Failed to load plugins:\n{ex.Message}",
                "OK"
            );
        }
    }
#endif


    // =========================================================
    // Built-in Command Registration
    // =========================================================
    private static void RegisterBuiltIns(EditorView editor)
    {
        CommandRegistry.Instance.Register(new EditorCommand(
            "file.new", "New File", "File", () => editor.NewFile()));

        CommandRegistry.Instance.Register(new EditorCommand(
            "file.open", "Open File", "File", () => editor.OpenFile()));

        CommandRegistry.Instance.Register(new EditorCommand(
            "file.save", "Save File", "File",
            () => editor.SaveFile())
        {
            CanExecute = () => editor.HasUnsavedChanges
        });

        CommandRegistry.Instance.Register(new EditorCommand(
            "file.saveAs", "Save File As", "File", () => editor.SaveFileAs()));

        CommandRegistry.Instance.Register(new EditorCommand(
            "view.toggleLineNumbers", "Toggle Line Numbers", "View",
            () => editor.ToggleLineNumbers()));

        CommandRegistry.Instance.Register(new EditorCommand(
            "view.toggleStatusBar", "Toggle Status Bar", "View",
            () => editor.ToggleStatusBar()));

        CommandRegistry.Instance.Register(new EditorCommand(
            "view.toggleWordWrap", "Toggle Word Wrap", "View",
            () => editor.ToggleWordWrap()));
    }


    // =========================================================
    // Menu
    // =========================================================
    private static MenuBar BuildMenu(EditorView editor)
    {
        return new MenuBar(new[]
        {
            new MenuBarItem("_File", new[]
            {
                new MenuItem("_New", "Ctrl+N", () => editor.NewFile(),
                    shortcut: Key.CtrlMask | Key.N),

                new MenuItem("_Open...", "Ctrl+O", () => editor.OpenFile(),
                    shortcut: Key.CtrlMask | Key.O),

                new MenuItem("_Save", "Ctrl+S", () => editor.SaveFile(),
                    shortcut: Key.CtrlMask | Key.S),

                new MenuItem("Save _As...", "Ctrl+Shift+S", () => editor.SaveFileAs(),
                    shortcut: Key.CtrlMask | Key.ShiftMask | Key.S),

                new MenuItem("C_lose", "", () => editor.CloseFile(),
                    shortcut: Key.AltMask | Key.L),

                new MenuItem("_Exit", "Ctrl+Q", () => Application.RequestStop(),
                    shortcut: Key.CtrlMask | Key.Q)
            }),

            new MenuBarItem("_Edit", new[]
            {
                new MenuItem(
                    "Command _Palette...",
                    "Ctrl+Shift+P",
                    ShowCommandPalette,
                    shortcut: Key.CtrlMask | Key.ShiftMask | Key.P)
            }),

            new MenuBarItem("_View", new[]
            {
                new MenuItem("Line Numbers", "", () => editor.ToggleLineNumbers())
                { CheckType = MenuItemCheckStyle.Checked },

                new MenuItem("_Status Bar", "", () => editor.ToggleStatusBar())
                { CheckType = MenuItemCheckStyle.Checked },

                new MenuItem("_Word Wrap", "", () => editor.ToggleWordWrap())
                { CheckType = MenuItemCheckStyle.Checked }
            }),

            new MenuBarItem("_Help", new[]
            {
                new MenuItem("_Documentation", "F1", () => { }),
                new MenuItem("_Keyboard Shortcuts", "", ShowShortcuts),
                new MenuItem("_About", "", ShowAbout)
            })
        });
    }


    // =========================================================
    // Dialog Helpers
    // =========================================================
    private static void ShowCommandPalette()
    {
        Application.Run(new CommandPaletteDialog());
    }

    private static void ShowShortcuts()
    {
        const string text =
            "Keyboard Shortcuts:\n\n" +
            "Ctrl+Shift+P - Command Palette\n" +
            "Ctrl+N - New File\n" +
            "Ctrl+O - Open File\n" +
            "Ctrl+S - Save File\n" +
            "Ctrl+Shift+S - Save As\n" +
            "Ctrl+Q - Exit\n";

        MessageBox.Query(60, 15, "Keyboard Shortcuts", text, "OK");
    }

    private static void ShowAbout()
    {
        MessageBox.Query(
            50, 10,
            "About Edit Sharp",
            "Edit Sharp v1.0\n\n" +
            "Lightweight terminal text editor\n" +
            "with Lua plugin support\n\n" +
            "Press Ctrl+Shift+P for Command Palette",
            "OK");
    }
}
