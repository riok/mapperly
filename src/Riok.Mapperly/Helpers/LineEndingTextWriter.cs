using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace Riok.Mapperly.Helpers;

/// <summary>
/// A TextWriter that replaces CRLF (\r\n) with a target line ending on-the-fly.
/// This avoids intermediate string allocations when converting line endings.
/// </summary>
public sealed class LineEndingTextWriter : TextWriter
{
    private readonly StringBuilder _sb;
    private readonly string _targetLineEnding;
    private bool _lastCharWasCr;

    public LineEndingTextWriter(StringBuilder sb, string targetLineEnding)
    {
        _sb = sb;
        _targetLineEnding = targetLineEnding;
    }

    /// <summary>
    /// Required by <see cref="TextWriter"/> but not used in this implementation.
    /// Character encoding is handled by <see cref="SourceText"/> after line ending conversion.
    /// </summary>
    public override Encoding Encoding => Encoding.UTF8;

    public override void Write(char value)
    {
        if (_lastCharWasCr)
        {
            _lastCharWasCr = false;
            if (value == '\n')
            {
                // CRLF detected - replace it with target line ending
                _sb.Append(_targetLineEnding);
                return;
            }

            // Was just CR, not CRLF - write the CR we held back
            _sb.Append('\r');
        }

        if (value == '\r')
        {
            // Hold CR - might be the start of CRLF
            _lastCharWasCr = true;
            return;
        }

        _sb.Append(value);
    }

    public override void Write(string? value)
    {
        if (value == null)
            return;

        foreach (var c in value)
        {
            Write(c);
        }
    }

    public override void Write(char[] buffer, int index, int count)
    {
        for (var i = index; i < index + count; i++)
        {
            Write(buffer[i]);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Flush any pending CR on dispose
            if (_lastCharWasCr)
            {
                _sb.Append('\r');
                _lastCharWasCr = false;
            }
        }

        base.Dispose(disposing);
    }
}
