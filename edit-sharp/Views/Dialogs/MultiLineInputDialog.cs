using System.Net.Mime;
using Terminal.Gui;

namespace edit_sharp.Views.Dialogs;

// Multi-line input dialog for larger text input
public class MultiLineInputDialog : Dialog
{
    private readonly TextView _inputView;
    private readonly Label _promptLabel;

    public string InputValue { get; private set; } = string.Empty;
    public bool Cancelled { get; private set; } = true;

    public MultiLineInputDialog(string title, string prompt, string defaultValue = "") : base(title, 70, 20)
    {
        _promptLabel = new Label(prompt)
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill() - 1
        };
        Add(_promptLabel);

        _inputView = new TextView
        {
            X = 1,
            Y = 3,
            Width = Dim.Fill() - 1,
            Height = Dim.Fill() - 5,
            Text = defaultValue
        };
        Add(_inputView);

        var okButton = new Button("OK")
        {
            X = Pos.Center() - 10,
            Y = Pos.Bottom(this) - 2,
            IsDefault = true
        };
        okButton.Clicked += () =>
        {
            InputValue = _inputView.Text.ToString() ?? string.Empty;
            Cancelled = false;
            Application.RequestStop();
        };

        var cancelButton = new Button("Cancel")
        {
            X = Pos.Center() + 2,
            Y = Pos.Bottom(this) - 2
        };
        cancelButton.Clicked += () =>
        {
            Cancelled = true;
           Application.RequestStop();
        };

        Add(okButton);
        Add(cancelButton);

        _inputView.SetFocus();
    }
}
