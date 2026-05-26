using NativeCanvasWord.Rendering;

namespace NativeCanvasWord;

public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();

        Text = "NativeCanvasWord";

        Width = 1200;

        Height = 1000;

        var editor = new D2DControl
        {
            Dock = DockStyle.Fill
        };

        Controls.Add(editor);
    }
}