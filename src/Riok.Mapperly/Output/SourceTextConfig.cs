namespace Riok.Mapperly.Output;

/// <summary>
/// Configuration for generated source text formatting, derived from .editorconfig settings.
/// </summary>
/// <param name="EndOfLine">The line ending style (lf, crlf, cr).</param>
/// <param name="Charset">The character encoding (utf-8, utf-8-bom, utf-16le, utf-16be).</param>
public readonly record struct SourceTextConfig(string? EndOfLine = null, string? Charset = null);
