using Terminal.Gui;

namespace edit_sharp.Views;public class LineNumberView : View
{
    private readonly TextView _textView;
    private int _lineNumberWidth = 4; 

    public LineNumberView(TextView textView)
    {
        _textView = textView ?? throw new ArgumentNullException(nameof(textView));
        Height = Dim.Fill();
        Width = _lineNumberWidth;

        _textView.TextChanged += SetNeedsDisplay;
        _textView.DrawContent += (_) => SetNeedsDisplay();
    }

    public override void Redraw(Rect bounds)
    {
        Driver.SetAttribute(ColorScheme?.Normal ?? Application.Current.ColorScheme.Normal);

        var totalLines = _textView.Lines;
        var visibleLines = _textView.Bounds.Height;
        var topLine = _textView.TopRow;


        var maxLineNumber = Math.Max(1, topLine + visibleLines);
        var digits = maxLineNumber.ToString().Length;
        _lineNumberWidth = digits + 2; 
        Width = _lineNumberWidth; 

        for (var i = 0; i < visibleLines; i++)
        {
            var lineNumber = topLine + i + 1;
            var numStr = lineNumber.ToString();

            var totalPadding = _lineNumberWidth - numStr.Length;
            var leftPadding = totalPadding / 2;
            var rightPadding = totalPadding - leftPadding;

            var paddedNumber = new string(' ', leftPadding) + numStr + new string(' ', rightPadding);

            Move(0, i);
            Driver.AddStr(paddedNumber);
        }
    }

    public int GetCalculatedWidth() => _lineNumberWidth;
}

