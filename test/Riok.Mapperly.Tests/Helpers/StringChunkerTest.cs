using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Tests.Helpers;

public class StringChunkerTest
{
    [Theory]
    [InlineData("camelCase", new[] { "camel", "Case" })]
    [InlineData("PascalCase", new[] { "Pascal", "Case" })]
    [InlineData("ABC", new[] { "A", "B", "C" })]
    [InlineData("abcABC", new[] { "abc", "A", "B", "C" })]
    public void ChunkPascalCaseShouldWork(string str, string[] expected)
    {
        StringChunker.ChunkPascalCase(str).Should().BeEquivalentTo(expected, o => o.WithStrictOrdering());
    }
}
