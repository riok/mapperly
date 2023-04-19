using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Tests.Helpers;

// can't use extension methods due to ambiguous method reference.
// (no support for this method in netstandard2.0)
public class DictionaryExtensionsTest
{
    [Fact]
    public void RemoveShouldReturnTrueWhenKeyWasRemoved()
    {
        var d = new Dictionary<string, int> { ["a"] = 10, ["b"] = 20, };
        DictionaryExtensions.Remove(d, "a", out var value).Should().BeTrue();
        value.Should().Be(10);
    }

    [Fact]
    public void RemoveShouldReturnFalseWhenKeyWasNotRemoved()
    {
        var d = new Dictionary<string, int> { ["a"] = 10, ["b"] = 20, };
        DictionaryExtensions.Remove(d, "c", out var value).Should().BeFalse();
        value.Should().Be(0);
    }

    [Fact]
    public void RemoveRangeShouldRemoveEntries()
    {
        var d = new Dictionary<string, int>
        {
            ["a"] = 10,
            ["b"] = 20,
            ["c"] = 30
        };
        d.RemoveRange(new[] { "a", "c" });
        d.Keys.Should().BeEquivalentTo("b");
    }

    [Fact]
    public void GetValueOrDefaultShouldReturnValueIfFound()
    {
        var d = new Dictionary<string, int> { ["a"] = 10, ["b"] = 20, };
        DictionaryExtensions.GetValueOrDefault(d, "a").Should().Be(10);
    }

    [Fact]
    public void GetValueOrDefaultShouldReturnDefaultForPrimitiveIfNotFound()
    {
        var d = new Dictionary<string, int> { ["a"] = 10, ["b"] = 20, };
        DictionaryExtensions.GetValueOrDefault(d, "c").Should().Be(0);
    }

    [Fact]
    public void GetValueOrDefaultShouldReturnDefaultForReferenceTypeIfNotFound()
    {
        var d = new Dictionary<string, Version> { ["a"] = new(), ["b"] = new(), };
        DictionaryExtensions.GetValueOrDefault(d, "c").Should().BeNull();
    }
}
