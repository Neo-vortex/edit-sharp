using Terminal.Gui;

namespace edit_sharp.Views.Dialogs;

public class MyCustomDialog : Dialog
{
    public MyCustomDialog()
    {
        var label = new Label("Hello from the plugin dialog!")
        {
            X = Pos.Center(),
            Y = Pos.Center() - 1
        };
        var okButton = new Button("OK")
        {
            X = Pos.Center(),
            Y = Pos.Center() + 1
        };
        okButton.Clicked += () => Application.RequestStop();

        Add(label, okButton);
    }
}

public class MyCustomDialog2 : Dialog
{
    public MyCustomDialog2()
    {
        var label = new Label("You opened a file!")
        {
            X = Pos.Center(),
            Y = Pos.Center() - 1
        };
        var okButton = new Button("OK")
        {
            X = Pos.Center(),
            Y = Pos.Center() + 1
        };
        okButton.Clicked += () => Application.RequestStop();

        Add(label, okButton);
    }
}

public class MyCustomDialog3 : Dialog
{
    public MyCustomDialog3()
    {
        var label = new Label("I see your text change from plugin!")
        {
            X = Pos.Center(),
            Y = Pos.Center() - 1
        };
        var okButton = new Button("OK")
        {
            X = Pos.Center(),
            Y = Pos.Center() + 1
        };
        okButton.Clicked += () => Application.RequestStop();

        Add(label, okButton);
    }
}