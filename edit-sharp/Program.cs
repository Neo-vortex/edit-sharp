using edit_sharp.Views;
using Terminal.Gui;

Application.Init();

var top = Application.Top ?? throw new InvalidOperationException("Terminal.GUI failed to initialize");


var menuColorScheme = new ColorScheme()
{
    Normal = Application.Driver.MakeAttribute(Color.Gray, Color.Blue),
    Focus = Application.Driver.MakeAttribute(Color.BrightGreen, Color.Gray),
    Disabled = Application.Driver.MakeAttribute(Color.DarkGray, Color.Blue),
    HotNormal = Application.Driver.MakeAttribute(Color.BrightBlue, Color.Blue),
    HotFocus = Application.Driver.MakeAttribute(Color.Green, Color.Gray)
};

var headerFooterColorScheme = new ColorScheme()
{
    Normal = Application.Driver.MakeAttribute(Color.White, Color.Black),
    Focus = Application.Driver.MakeAttribute(Color.BrightYellow, Color.Black)
};

var editorColorScheme = new ColorScheme()
{
    Normal = Application.Driver.MakeAttribute(Color.White, Color.Black),
    Focus = Application.Driver.MakeAttribute(Color.BrightYellow, Color.Black),
    Disabled = Application.Driver.MakeAttribute(Color.Gray, Color.Black)
};


var window = new Window("Edit Sharp")
{
    X = 0,
    Y = 1, 
    Width = Dim.Fill(),
    Height = Dim.Fill(),
    ColorScheme = headerFooterColorScheme
};


var editor = new EditorView(window)
{
    TextView =
    {
        ColorScheme = editorColorScheme
    },
    MenuBar = new MenuBar([])
};
editor.TextView.ColorScheme = editorColorScheme;
top.Add(window);

var menuBar = new MenuBar([
    new MenuBarItem("_File", [
        new MenuItem("_New", "Ctrl+N", () => editor.NewFile(), shortcut: Key.CtrlMask | Key.N),
        new MenuItem("_Open...", "Ctrl+O", () => editor.OpenFile(), shortcut: Key.CtrlMask | Key.O),
        new MenuItem("_Save", "Ctrl+S", () => editor.SaveFile(), shortcut: Key.CtrlMask | Key.S),
        new MenuItem("Save _As...", "Ctrl+Shift+S", () => editor.SaveFileAs(), shortcut: Key.CtrlMask | Key.ShiftMask | Key.S),
        new MenuItem("C_lose", "", () => editor.CloseFile(), shortcut: Key.AltMask | Key.L),
        new MenuItem("_Exit", "Ctrl+Q", () => Application.RequestStop(), shortcut: Key.CtrlMask | Key.Q)
    ]),
    new MenuBarItem("_View", [
        new MenuItem("Line Numbers", "", () => editor.ToggleLineNumbers()) { CheckType = MenuItemCheckStyle.Checked },
        new MenuItem("_Status Bar", "", () => editor.ToggleStatusBar()) { CheckType = MenuItemCheckStyle.Checked },
        new MenuItem("_Word Wrap", "", () => editor.ToggleWordWrap()) { CheckType = MenuItemCheckStyle.Checked }
    ]),
    new MenuBarItem("_Help", [
        new MenuItem("_Documentation", "F1", () => { }),
        new MenuItem("_Keyboard Shortcuts", "", () => { }),
        new MenuItem("_About", "", () => { })
    ])
])
{
    ColorScheme = menuColorScheme
};

editor.MenuBar = menuBar;
top.Add(menuBar);

Application.Run(top);
