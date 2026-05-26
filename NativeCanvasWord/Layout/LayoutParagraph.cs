using Vortice.DirectWrite;

namespace NativeCanvasWord.Layout;

public class LayoutParagraph
{
    public IDWriteTextLayout TextLayout { get; }

    public float X { get; }

    public float Y { get; }

    public LayoutParagraph(
        IDWriteTextLayout layout,
        float x,
        float y)
    {
        TextLayout = layout;

        X = x;

        Y = y;
    }
}