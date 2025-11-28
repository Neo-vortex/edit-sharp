using Terminal.Gui;

namespace edit_sharp.Views.Dialogs;

public class InputDialog : Dialog
{
    private readonly TextField _inputField;
    private readonly Label _promptLabel;

    public string InputValue { get; private set; } = string.Empty;
    public bool Cancelled { get; private set; } = true;

    public InputDialog(string title, string prompt, string defaultValue = "") : base(title, 60, 10)
    {
        _promptLabel = new Label(prompt)
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill() - 1
        };
        Add(_promptLabel);

        _inputField = new TextField(defaultValue)
        {
            X = 1,
            Y = 3,
            Width = Dim.Fill() - 1
        };
        Add(_inputField);

        var okButton = new Button("OK")
        {
            X = Pos.Center() - 10,
            Y = Pos.Bottom(this) - 4,
            IsDefault = true
        };
        okButton.Clicked += () =>
        {
            InputValue = _inputField.Text.ToString() ?? string.Empty;
            Cancelled = false;
            Application.RequestStop();
        };

        var cancelButton = new Button("Cancel")
        {
            X = Pos.Center() + 2,
            Y = Pos.Bottom(this) - 4
        };
        cancelButton.Clicked += () =>
        {
            Cancelled = true;
            Application.RequestStop();
        };

        Add(okButton);
        Add(cancelButton);

        _inputField.SetFocus();
    }
}