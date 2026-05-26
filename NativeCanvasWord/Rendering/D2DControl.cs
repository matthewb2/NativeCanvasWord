using System;
using System.Windows.Forms;

namespace NativeCanvasWord.Rendering;

public class D2DControl : Control
{
    private D2DRenderer? _renderer;

    public D2DControl()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.Opaque,
            true);

        Resize += OnResize;
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);

        _renderer = new D2DRenderer(this);
    }

    private void OnResize(object? sender, EventArgs e)
    {
        _renderer?.Resize(Width, Height);

        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        _renderer?.Render();
    }
}