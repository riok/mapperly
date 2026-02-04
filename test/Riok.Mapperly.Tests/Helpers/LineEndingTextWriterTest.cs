using System.Text;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Tests.Helpers;

public class LineEndingTextWriterTest
{
    [Fact]
    public void ShouldConvertCrlfToLf()
    {
        var result = Convert("Hello\r\nWorld\r\n", "\n");
        result.ShouldBe("Hello\nWorld\n");
    }

    [Fact]
    public void ShouldConvertCrlfToCr()
    {
        var result = Convert("Hello\r\nWorld\r\n", "\r");
        result.ShouldBe("Hello\rWorld\r");
    }

    [Fact]
    public void ShouldHandleTextWithNoLineEndings()
    {
        var result = Convert("HelloWorld", "\n");
        result.ShouldBe("HelloWorld");
    }

    [Fact]
    public void ShouldPreserveLoneCarriageReturn()
    {
        // Standalone CR (not part of CRLF) should be preserved
        var result = Convert("Hello\rWorld", "\n");
        result.ShouldBe("Hello\rWorld");
    }

    [Fact]
    public void ShouldPreserveLoneLineFeed()
    {
        // Standalone LF (not part of CRLF) should be preserved
        var result = Convert("Hello\nWorld", "\n");
        result.ShouldBe("Hello\nWorld");
    }

    [Fact]
    public void ShouldHandleMixedLineEndings()
    {
        // Mix of CRLF, LF, and CR
        var result = Convert("A\r\nB\nC\rD\r\nE", "\n");
        result.ShouldBe("A\nB\nC\rD\nE");
    }

    [Fact]
    public void ShouldHandleEmptyString()
    {
        var result = Convert("", "\n");
        result.ShouldBe("");
    }

    [Fact]
    public void ShouldHandleNullString()
    {
        var sb = new StringBuilder();
        using (var writer = new LineEndingTextWriter(sb, "\n"))
        {
            writer.Write((string?)null);
        }

        sb.ToString().ShouldBe("");
    }

    [Fact]
    public void ShouldHandleConsecutiveCrlfSequences()
    {
        var result = Convert("A\r\n\r\n\r\nB", "\n");
        result.ShouldBe("A\n\n\nB");
    }

    [Fact]
    public void ShouldHandleTrailingCrAtEndOfText()
    {
        // CR at end of text (not followed by LF) should be preserved on dispose
        var result = Convert("Hello\r", "\n");
        result.ShouldBe("Hello\r");
    }

    [Fact]
    public void ShouldHandleOnlyCrlf()
    {
        var result = Convert("\r\n", "\n");
        result.ShouldBe("\n");
    }

    [Fact]
    public void ShouldWorkWithCharArrayWrite()
    {
        var sb = new StringBuilder();
        var chars = "Hello\r\nWorld".ToCharArray();
        using (var writer = new LineEndingTextWriter(sb, "\n"))
        {
            writer.Write(chars, 0, chars.Length);
        }

        sb.ToString().ShouldBe("Hello\nWorld");
    }

    [Fact]
    public void ShouldWorkWithPartialCharArrayWrite()
    {
        var sb = new StringBuilder();
        var chars = "XXXHello\r\nWorldXXX".ToCharArray();
        using (var writer = new LineEndingTextWriter(sb, "\n"))
        {
            writer.Write(chars, 3, 12); // "Hello\r\nWorld"
        }

        sb.ToString().ShouldBe("Hello\nWorld");
    }

    [Fact]
    public void ShouldHandleLargeText()
    {
        // Build a large text with many CRLF sequences
        var lines = Enumerable.Range(0, 1000).Select(i => $"Line {i}").ToArray();
        var input = string.Join("\r\n", lines);
        var expected = string.Join("\n", lines);

        var result = Convert(input, "\n");
        result.ShouldBe(expected);
    }

    [Fact]
    public void ShouldHandleCrlfSplitAcrossWrites()
    {
        var sb = new StringBuilder();
        using (var writer = new LineEndingTextWriter(sb, "\n"))
        {
            writer.Write("Hello\r");
            writer.Write("\nWorld");
        }

        sb.ToString().ShouldBe("Hello\nWorld");
    }

    [Fact]
    public void ShouldHandleCrFollowedByNonLfAcrossWrites()
    {
        var sb = new StringBuilder();
        using (var writer = new LineEndingTextWriter(sb, "\n"))
        {
            writer.Write("Hello\r");
            writer.Write("World"); // Not starting with \n
        }

        sb.ToString().ShouldBe("Hello\rWorld");
    }

    private static string Convert(string input, string targetLineEnding)
    {
        var sb = new StringBuilder();
        using (var writer = new LineEndingTextWriter(sb, targetLineEnding))
        {
            writer.Write(input);
        }

        return sb.ToString();
    }
}
