using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Riok.Mapperly.Tests;

/// <summary>
/// A test implementation of <see cref="AnalyzerConfigOptionsProvider"/> that provides
/// editorconfig settings for testing purposes.
/// </summary>
internal class TestAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
{
    private readonly TestAnalyzerConfigOptions _fileOptions;

    public TestAnalyzerConfigOptionsProvider(string? endOfLine, string? charset)
    {
        _fileOptions = new TestAnalyzerConfigOptions(endOfLine, charset);
    }

    // GlobalOptions should NOT contain .editorconfig settings - they're per-file only
    public override AnalyzerConfigOptions GlobalOptions => TestAnalyzerConfigOptions.Empty;

    public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => _fileOptions;

    public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => _fileOptions;
}

/// <summary>
/// A test implementation of <see cref="AnalyzerConfigOptions"/> that provides
/// specific editorconfig values for testing.
/// </summary>
internal class TestAnalyzerConfigOptions : AnalyzerConfigOptions
{
    public static readonly TestAnalyzerConfigOptions Empty = new(null, null);

    private readonly ImmutableDictionary<string, string> _options;

    public TestAnalyzerConfigOptions(string? endOfLine, string? charset)
    {
        _options = new Dictionary<string, string?> { ["end_of_line"] = endOfLine, ["charset"] = charset }
            .Where(x => x.Value != null)
            .ToImmutableDictionary(x => x.Key, x => x.Value!);
    }

    public override bool TryGetValue(string key, out string value) => _options.TryGetValue(key, out value!);
}
