using NativeCanvasWord.Document;
using NativeCanvasWord.Layout;
using System.Numerics;
using Vortice.Direct2D1;
using Vortice.DirectWrite;
using Vortice.Mathematics;
using static Vortice.Direct2D1.D2D1;
using static Vortice.DirectWrite.DWrite;

namespace NativeCanvasWord.Rendering;

public class D2DRenderer
{
    private readonly Control _control;

    private ID2D1Factory? _factory;

    private ID2D1HwndRenderTarget? _renderTarget;

    private ID2D1SolidColorBrush? _blackBrush;

    private ID2D1SolidColorBrush? _whiteBrush;

    private IDWriteFactory? _writeFactory;

    private DocumentModel _document;

    private LayoutPage _layoutPage = new();


    public D2DRenderer(Control control)
    {
        _control = control;

        Initialize();
    }

    private void BuildLayout()
    {
        if (_writeFactory == null)
            return;

        _layoutPage = new LayoutPage();

        float pageX = 100;

        float pageY = 60;

        float marginLeft = 80;

        float marginTop = 80;

        float contentWidth = 794 - 160;

        float currentY =
            pageY + marginTop;

        foreach (var paragraph in _document.Paragraphs)
        {
            string text =
                string.Concat(
                    paragraph.Runs.Select(x => x.Text));

            var format =
                _writeFactory.CreateTextFormat(
                    "맑은 고딕",
                    18);

            var layout =
                _writeFactory.CreateTextLayout(
                    text,
                    format,
                    contentWidth,
                    2000);

            var lp =
                new LayoutParagraph(
                    layout,
                    pageX + marginLeft,
                    currentY);

            _layoutPage.Paragraphs.Add(lp);

            currentY +=
                layout.Metrics.Height + 24;
        }
    }


    private void Initialize()
    {
        _factory = D2D1CreateFactory<ID2D1Factory>();

        _writeFactory =
    DWriteCreateFactory<IDWriteFactory>();

        _document =
            DocumentModel.CreateSample();

        BuildLayout();


        var renderProps = new RenderTargetProperties();

        var hwndProps = new HwndRenderTargetProperties
        {
            Hwnd = _control.Handle,

            PixelSize = new Vortice.Mathematics.SizeI(
                Math.Max(_control.Width, 1),
                Math.Max(_control.Height, 1))
        };

        _renderTarget =
            _factory.CreateHwndRenderTarget(
                renderProps,
                hwndProps);

        _blackBrush =
            _renderTarget.CreateSolidColorBrush(
                new Color4(0, 0, 0, 1));

        _whiteBrush =
            _renderTarget.CreateSolidColorBrush(
                new Color4(1, 1, 1, 1));
    }

    public void Resize(int width, int height)
    {
        _renderTarget?.Resize(
            new SizeI(
                Math.Max(width, 1),
                Math.Max(height, 1)));
    }

    public void Render()
    {
        if (_renderTarget == null)
            return;

        _renderTarget.BeginDraw();

        _renderTarget.Clear(
            new Color4(0.85f, 0.85f, 0.85f, 1));

        DrawPage();

        _renderTarget.EndDraw();
    }

    private void DrawPage()
    {
        if (_renderTarget == null ||
            _whiteBrush == null ||
            _blackBrush == null)
            return;

        float pageX = 100;

        float pageY = 60;

        float pageWidth = 794;

        float pageHeight = 1123;

        _renderTarget.FillRectangle(
            new Rect(
                pageX,
                pageY,
                pageX + pageWidth,
                pageY + pageHeight),
            _whiteBrush);

        _renderTarget.DrawRectangle(
            new Rect(
                pageX,
                pageY,
                pageX + pageWidth,
                pageY + pageHeight),
            _blackBrush,
            1);

        DrawDocument();

    }

    private void DrawDocument()
    {
        if (_renderTarget == null ||
            _blackBrush == null)
            return;

        foreach (var p in _layoutPage.Paragraphs)
        {
            _renderTarget.DrawTextLayout(
                new Vector2(
                    p.X,
                    p.Y),
                p.TextLayout,
                _blackBrush);
        }
    }

}