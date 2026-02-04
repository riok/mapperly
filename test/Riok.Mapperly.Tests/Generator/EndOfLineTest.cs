using Microsoft.CodeAnalysis.Text;
using Riok.Mapperly.Diagnostics;

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

    [Theory]
    [InlineData("unknown-charset")]
    [InlineData("ascii")]
    [InlineData("latin1")]
    public void ShouldDiagnoseUnknownCharset(string unknownCharset)
    {
        var source = TestSourceBuilder.Mapping("string", "string");
        var options = TestHelperOptions.AllowDiagnostics with { EditorConfigCharset = unknownCharset };
        var result = TestHelper.GenerateMapper(source, options);

        result
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.UnknownEditorConfigCharset,
                $"Unknown charset value '{unknownCharset}' in editorconfig, valid values are: utf-8, utf-8-bom, utf-16be, utf-16le"
            );
    }

    [Theory]
    [InlineData("unknown-eol")]
    [InlineData("unix")]
    [InlineData("windows")]
    public void ShouldDiagnoseUnknownEndOfLine(string unknownEndOfLine)
    {
        var source = TestSourceBuilder.Mapping("string", "string");
        var options = TestHelperOptions.AllowDiagnostics with { EditorConfigEndOfLine = unknownEndOfLine };
        var result = TestHelper.GenerateMapper(source, options);

        result
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.UnknownEditorConfigEndOfLine,
                $"Unknown end_of_line value '{unknownEndOfLine}' in editorconfig, valid values are: lf, crlf, cr"
            );
    }

    [Theory]
    [InlineData("utf-8")]
    [InlineData("utf-8-bom")]
    [InlineData("utf-16le")]
    [InlineData("utf-16be")]
    [InlineData(null)]
    public void ShouldNotDiagnoseValidCharset(string? charset)
    {
        var source = TestSourceBuilder.Mapping("string", "string");
        var options = TestHelperOptions.Default with { EditorConfigCharset = charset };
        var result = TestHelper.GenerateMapper(source, options);

        result.Diagnostics.ShouldNotContain(d => d.Id == DiagnosticDescriptors.UnknownEditorConfigCharset.Id);
    }

    [Theory]
    [InlineData("lf")]
    [InlineData("crlf")]
    [InlineData("cr")]
    [InlineData(null)]
    public void ShouldNotDiagnoseValidEndOfLine(string? endOfLine)
    {
        var source = TestSourceBuilder.Mapping("string", "string");
        var options = TestHelperOptions.Default with { EditorConfigEndOfLine = endOfLine };
        var result = TestHelper.GenerateMapper(source, options);

        result.Diagnostics.ShouldNotContain(d => d.Id == DiagnosticDescriptors.UnknownEditorConfigEndOfLine.Id);
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
