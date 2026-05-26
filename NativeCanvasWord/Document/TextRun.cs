namespace NativeCanvasWord.Document;

public class TextRun
{
    public string Text { get; set; }

    public float FontSize { get; set; } = 18;

    public string FontFamily { get; set; } = "맑은 고딕";

    public TextRun(string text)
    {
        Text = text;
    }
}