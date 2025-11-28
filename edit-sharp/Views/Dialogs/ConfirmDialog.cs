using Terminal.Gui;

namespace edit_sharp.Views.Dialogs;


// Confirmation dialog
public class ConfirmDialog : Dialog
{
    public bool Confirmed { get; private set; } = false;

    public ConfirmDialog(string title, string message) : base(title, 60, 10)
    {
        var messageLabel = new Label(message)
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill() - 1,
            Height = Dim.Fill() - 5
        };
        Add(messageLabel);

        var yesButton = new Button("Yes")
        {
            X = Pos.Center() - 10,
            Y = Pos.Bottom(this) - 3,
            IsDefault = true
        };
        yesButton.Clicked += () =>
        {
            Confirmed = true;
            Application.RequestStop();
        };

        var noButton = new Button("No")
        {
            X = Pos.Center() + 2,
            Y = Pos.Bottom(this) - 3
        };
        noButton.Clicked += () =>
        {
            Confirmed = false;
            Application.RequestStop();
        };

        Add(yesButton);
        Add(noButton);
    }
}