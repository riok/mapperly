using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Tests.Helpers;

// can't use extension methods due to ambiguous method reference.
// (no support for this method in netstandard2.0)
public class DictionaryExtensionsTest
{
    [Fact]
    public void RemoveShouldReturnTrueWhenKeyWasRemoved()
    {
        var d = new Dictionary<string, int> { ["a"] = 10, ["b"] = 20 };
        DictionaryExtensions.Remove(d, "a", out var value).ShouldBeTrue();
        value.ShouldBe(10);
    }

    [Fact]
    public void RemoveShouldReturnFalseWhenKeyWasNotRemoved()
    {
        var d = new Dictionary<string, int> { ["a"] = 10, ["b"] = 20 };
        DictionaryExtensions.Remove(d, "c", out var value).ShouldBeFalse();
        value.ShouldBe(0);
    }

    [Fact]
    public void RemoveRangeShouldRemoveEntries()
    {
        var d = new Dictionary<string, int>
        {
            ["a"] = 10,
            ["b"] = 20,
            ["c"] = 30,
        };
        d.RemoveRange(["a", "c"]);
        d.Keys.ShouldBe(["b"]);
    }

    [Fact]
    public void TryAddShouldNotAddExistingKey()
    {
        var d = new Dictionary<string, int> { ["a"] = 10 };

        d.TryAdd("a", 20).ShouldBeFalse();

        d["a"].ShouldBe(10);
    }

    [Fact]
    public void TryAddShouldAddNewKey()
    {
        var d = new Dictionary<string, int> { ["a"] = 10 };

        d.TryAdd("b", 20).ShouldBeTrue();

        d["b"].ShouldBe(20);
    }
}
