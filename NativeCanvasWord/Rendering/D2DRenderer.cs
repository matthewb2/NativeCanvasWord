using NativeCanvasWord.Document;
using NativeCanvasWord.Layout;
using System.Numerics;
using Vortice.Direct2D1;
using Vortice.DirectWrite;
using Vortice.Mathematics;
using static Vortice.Direct2D1.D2D1;
using static Vortice.DirectWrite.DWrite;
using NativeCanvasWord.Input;
using SharpGen.Runtime;



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

    private readonly CaretState _caret = new();

    private float _preferredCaretX;



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

            currentY += layout.Metrics.Height + 24;


        }
        UpdateCaretFromTextPosition();
    }

    private LineMetrics[] GetLineMetrics(
    IDWriteTextLayout layout)
    {
        uint count = 32;

        var metrics =
            new LineMetrics[count];

        layout.GetLineMetrics(
            metrics,
            out uint actualCount);

        if (actualCount < count)
        {
            Array.Resize(
                ref metrics,
                (int)actualCount);
        }

        return metrics;
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

        DrawCaret();

        _renderTarget.EndDraw();
    }

    public void HandleTextInput(char c)
    {
        if (_caret.ParagraphIndex <
            0 ||
            _caret.ParagraphIndex >=
            _document.Paragraphs.Count)
            return;

        var paragraph =
            _document.Paragraphs[
                _caret.ParagraphIndex];

        if (paragraph.Runs.Count == 0)
            return;

        var run = paragraph.Runs[0];

        run.Text =
            run.Text.Insert(
                (int)_caret.TextPosition,
                c.ToString());

        _caret.TextPosition++;

        BuildLayout();

        UpdateCaretFromTextPosition();
    }

    private void UpdateCaretFromTextPosition()
    {
        if (_caret.ParagraphIndex <
            0 ||
            _caret.ParagraphIndex >=
            _layoutPage.Paragraphs.Count)
            return;

        var p =
            _layoutPage.Paragraphs[
                _caret.ParagraphIndex];

        p.TextLayout.HitTestTextPosition(
            _caret.TextPosition,
            false,
            out float x,
            out float y,
            out var metrics);

        _caret.X =
            p.X + x;

        _caret.Y =
            p.Y + y;
        
        _caret.Height =
            Math.Max(metrics.Height, 24);

        _preferredCaretX = _caret.X;
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

    public void HandleMouseDown(
    float mouseX,
    float mouseY)
    {
        foreach (var p in _layoutPage.Paragraphs
                     .Select((x, i) => new { x, i }))
        {
            var layout = p.x.TextLayout;

            float localX =
                mouseX - p.x.X;

            float localY =
                mouseY - p.x.Y;

            layout.HitTestPoint(
                localX,
                localY,
                out RawBool trailing,
                out RawBool inside,
                out var metrics);

            if (!inside)
                continue;

            _caret.ParagraphIndex = p.i;

            _caret.TextPosition =
                metrics.TextPosition +
                (trailing ? 1u : 0);

            _caret.X =
                p.x.X + metrics.Left;

            _caret.Y =
                p.x.Y + metrics.Top;

            _caret.Height =
                metrics.Height;

            break;
        }
        _preferredCaretX = _caret.X; // 마우스 클릭 시 preferred X 갱신
    }

    private void DrawCaret()
    {
        if (_renderTarget == null ||
            _blackBrush == null)
            return;

        _renderTarget.DrawLine(
            new Vector2(
                _caret.X,
                _caret.Y),
            new Vector2(
                _caret.X,
                _caret.Y + _caret.Height),
            _blackBrush,
            1.5f);
    }

    public void HandleKeyDown(Keys key)
    {
        switch (key)
        {
            case Keys.Left:
                MoveCaretLeft();
                break;

            case Keys.Right:
                MoveCaretRight();
                break;

            case Keys.Up:
                MoveCaretVertical(-1);
                break;

            case Keys.Down:
                MoveCaretVertical(1);
                break;

            case Keys.Home:
                MoveCaretHome();
                break;

            case Keys.End:
                MoveCaretEnd();
                break;
        }
    }

    private void MoveCaretLeft()
    {
        if (_caret.TextPosition > 0)
        {
            _caret.TextPosition--;

            UpdateCaretFromTextPosition();

            _preferredCaretX = _caret.X;
        }
    }

    private void MoveCaretRight()
    {
        var paragraph =
            _document.Paragraphs[
                _caret.ParagraphIndex];

        var text =
            string.Concat(
                paragraph.Runs.Select(x => x.Text));

        if (_caret.TextPosition < text.Length)
        {
            _caret.TextPosition++;

            UpdateCaretFromTextPosition();

            _preferredCaretX = _caret.X;
        }
    }

    private void MoveCaretHome()
    {
        var p =
            _layoutPage.Paragraphs[
                _caret.ParagraphIndex];

        var layout =
            p.TextLayout;

        var lines =
            GetLineMetrics(layout);

        uint currentPosition = 0;

        foreach (var line in lines)
        {
            uint lineStart =
                currentPosition;

            uint lineEnd =
                currentPosition +
                line.Length;

            if (_caret.TextPosition >= lineStart &&
                _caret.TextPosition <= lineEnd)
            {
                _caret.TextPosition =
                    lineStart;

                UpdateCaretFromTextPosition();

                return;
            }

            currentPosition += line.Length;
        }
    }

    private void MoveCaretEnd()
    {
        var p =
            _layoutPage.Paragraphs[
                _caret.ParagraphIndex];

        var layout =
            p.TextLayout;

        var lines =
            GetLineMetrics(layout);

        uint currentStart = 0;

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            uint nextStart =
                currentStart + line.Length;

            bool isCurrentLine =
                _caret.TextPosition >= currentStart &&
                _caret.TextPosition < nextStart;

            if (isCurrentLine)
            {
                uint visualEnd =
                    nextStart;

                // 줄바꿈 문자 제외
                if (line.NewlineLength > 0)
                {
                    visualEnd -=
                        (uint)line.NewlineLength;
                }

                // 마지막 실제 문자 뒤 위치
                if (visualEnd > currentStart)
                {
                    visualEnd--;
                }

                _caret.TextPosition =
                    visualEnd;

                UpdateCaretFromTextPosition();

                return;
            }

            currentStart = nextStart;
        }
    }

    private void MoveCaretVertical(
    int direction)
    {
        var p =
            _layoutPage.Paragraphs[
                _caret.ParagraphIndex];

        float localY =
            _caret.Y - p.Y;

        localY += direction * 24;

        p.TextLayout.HitTestPoint(
            _preferredCaretX - p.X,
            localY,
            out var trailing,
            out var inside,
            out var metrics);

        _caret.TextPosition =
            (uint)metrics.TextPosition;

        if (trailing)
            _caret.TextPosition++;

        UpdateCaretFromTextPosition();
    }



}