using System;
using System.Windows.Forms;

namespace NativeCanvasWord.Rendering;

public class D2DControl : Control
{
    private D2DRenderer? _renderer;

    public D2DControl()
    {
        TabStop = true;
        PreviewKeyDown += D2DControl_PreviewKeyDown;

        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.Opaque,
            true);

        Resize += OnResize;
    }

    private void D2DControl_PreviewKeyDown(
    object? sender,
    PreviewKeyDownEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.Left:
            case Keys.Right:
            case Keys.Up:
            case Keys.Down:
            case Keys.Home:
            case Keys.End:
                e.IsInputKey = true;
                break;
        }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        Focus();

        _renderer?.HandleMouseDown(e.X, e.Y);

        Invalidate();
    }

    protected override void OnKeyPress(KeyPressEventArgs e)
    {
        base.OnKeyPress(e);

        _renderer?.HandleTextInput(e.KeyChar);

        Invalidate();
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

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        _renderer?.HandleKeyDown(e.KeyCode);

        Invalidate();
    }

}