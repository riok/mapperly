using System.Text;

namespace Riok.Mapperly.Output;

/// <summary>
/// Configuration for generated source text formatting, derived from .editorconfig settings.
/// </summary>
/// <param name="EndOfLine">The actual line ending string ("\n", "\r", "\r\n"), or null for default (CRLF).</param>
/// <param name="Encoding">The character encoding to use for the generated source.</param>
public readonly record struct SourceTextConfig(string? EndOfLine, Encoding Encoding);
