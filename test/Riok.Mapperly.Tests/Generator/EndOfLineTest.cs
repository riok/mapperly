using Microsoft.CodeAnalysis.Text;

namespace Riok.Mapperly.Tests.Generator;

public class EndOfLineTest
{
    [Theory]
    [InlineData(null, "\r\n")]
    [InlineData("lf", "\n")]
    [InlineData("crlf", "\r\n")]
    [InlineData("cr", "\r")]
    public void ShouldRespectEditorConfigEndOfLine(string? endOfLineSetting, string expectedLineEnding)
    {
        var generatedSource = GenerateSource(endOfLine: endOfLineSetting);

        generatedSource.ShouldContain(expectedLineEnding);

        // After removing expected line endings, no line ending chars should remain
        var normalized = generatedSource.Replace(expectedLineEnding, "");
        normalized.ShouldNotContain("\r");
        normalized.ShouldNotContain("\n");
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("utf-8", false)]
    [InlineData("utf-8-bom", true)]
    public void ShouldRespectEditorConfigCharset(string? charsetSetting, bool expectBom)
    {
        var generatedText = GenerateSourceText(charset: charsetSetting);
        var preamble = generatedText.Encoding?.GetPreamble() ?? [];

        preamble.Length.ShouldBe(expectBom ? 3 : 0, $"charset={charsetSetting ?? "(default)"} should {(expectBom ? "" : "not ")}have BOM");
    }

    private static string GenerateSource(string? endOfLine = null, string? charset = null) =>
        GenerateSourceText(endOfLine, charset).ToString();

    private static SourceText GenerateSourceText(string? endOfLine = null, string? charset = null)
    {
        var source = TestSourceBuilder.Mapping("string", "string");
        var options = TestHelperOptions.Default with { EditorConfigEndOfLine = endOfLine, EditorConfigCharset = charset };
        return TestHelper.GenerateSourceText(source, options);
    }
}
