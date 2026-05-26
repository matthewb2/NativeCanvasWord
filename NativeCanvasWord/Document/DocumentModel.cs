namespace NativeCanvasWord.Document;

public class DocumentModel
{
    public List<Paragraph> Paragraphs { get; } = [];

    public static DocumentModel CreateSample()
    {
        var doc = new DocumentModel();

        var p1 = new Paragraph();

        p1.Runs.Add(new TextRun(
            "NativeCanvasWord는 DirectWrite 기반의 페이지형 워드프로세서 엔진입니다. " +
            "이 문장은 자동 줄바꿈 테스트를 위해 길게 작성되었습니다. " +
            "브라우저 기반 canvas-editor를 네이티브 Windows 엔진으로 포팅하는 중입니다."));

        doc.Paragraphs.Add(p1);

        var p2 = new Paragraph();

        p2.Runs.Add(new TextRun(
            "두 번째 단락입니다.\n\n" +
            "향후 caret, selection, pagination, IME가 추가됩니다."));

        doc.Paragraphs.Add(p2);

        return doc;
    }
}