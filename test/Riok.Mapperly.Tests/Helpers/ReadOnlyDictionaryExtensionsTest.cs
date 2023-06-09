using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Tests.Helpers;

// can't use extension methods due to ambiguous method reference.
// (no support for this method in netstandard2.0)
public class ReadOnlyDictionaryExtensionsTest
{
    [Fact]
    public void GetValueOrDefaultShouldReturnValueIfFound()
    {
        var d = new Dictionary<string, int> { ["a"] = 10, ["b"] = 20, };
        ReadOnlyDictionaryExtensions.GetValueOrDefault(d, "a").Should().Be(10);
    }

    [Fact]
    public void GetValueOrDefaultShouldReturnDefaultForPrimitiveIfNotFound()
    {
        var d = new Dictionary<string, int> { ["a"] = 10, ["b"] = 20, };
        ReadOnlyDictionaryExtensions.GetValueOrDefault(d, "c").Should().Be(0);
    }

    [Fact]
    public void GetValueOrDefaultShouldReturnDefaultForReferenceTypeIfNotFound()
    {
        var d = new Dictionary<string, Version> { ["a"] = new(), ["b"] = new(), };
        ReadOnlyDictionaryExtensions.GetValueOrDefault(d, "c").Should().BeNull();
    }
}
