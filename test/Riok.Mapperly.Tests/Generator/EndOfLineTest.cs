using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
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
        var syntaxTree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default);
        var compilation = TestHelper.BuildCompilation(syntaxTree);
        var configOptionsProvider = new TestAnalyzerConfigOptionsProvider(endOfLine, charset);

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new MapperGenerator().AsSourceGenerator()],
            optionsProvider: configOptionsProvider
        );
        driver = driver.RunGenerators(compilation);

        return driver.GetRunResult().GeneratedTrees.First().GetText();
    }

    private class TestAnalyzerConfigOptionsProvider(string? endOfLine, string? charset) : AnalyzerConfigOptionsProvider
    {
        private readonly TestAnalyzerConfigOptions _fileOptions = new(endOfLine, charset);

        // GlobalOptions should NOT contain .editorconfig settings - they're per-file only
        public override AnalyzerConfigOptions GlobalOptions => new TestAnalyzerConfigOptions(null, null);

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => _fileOptions;

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => _fileOptions;
    }

    private class TestAnalyzerConfigOptions(string? endOfLine, string? charset) : AnalyzerConfigOptions
    {
        private readonly ImmutableDictionary<string, string> _options = new Dictionary<string, string?>
        {
            ["end_of_line"] = endOfLine,
            ["charset"] = charset,
        }
            .Where(x => x.Value != null)
            .ToImmutableDictionary(x => x.Key, x => x.Value!);

        public override bool TryGetValue(string key, out string value) => _options.TryGetValue(key, out value!);
    }
}
