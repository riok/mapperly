using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Tests.Helpers;

public class ListDictionaryTests
{
    [Fact]
    public void AddAndGetOrEmptyShouldAddValueAndReturn()
    {
        var l = new ListDictionary<int, int>();
        l.Add(1, 1);
        l.GetOrEmpty(1).Should().BeEquivalentTo([1], x => x.WithStrictOrdering());
        l.Add(1, 2);
        l.GetOrEmpty(1).Should().BeEquivalentTo([1, 2], x => x.WithStrictOrdering());
        l.Add(2, 3);
        l.GetOrEmpty(1).Should().BeEquivalentTo([1, 2], x => x.WithStrictOrdering());
        l.GetOrEmpty(2).Should().BeEquivalentTo([3], x => x.WithStrictOrdering());
    }

    [Fact]
    public void GetOrEmptyForEmptyShouldReturnEmpty()
    {
        var l = new ListDictionary<int, int>();
        l.GetOrEmpty(1).Should().BeEmpty();
    }

    [Fact]
    public void ContainsKey()
    {
        var l = new ListDictionary<int, int>();
        l.ContainsKey(1).Should().BeFalse();
        l.Add(1, 1);
        l.ContainsKey(1).Should().BeTrue();
    }

    [Fact]
    public void Remove()
    {
        var l = new ListDictionary<int, int>();
        l.Add(1, 1);
        l.ContainsKey(1).Should().BeTrue();
        l.Remove(1);
        l.ContainsKey(1).Should().BeFalse();
    }

    [Fact]
    public void RemoveUnknown()
    {
        var l = new ListDictionary<int, int>();
        l.Remove(1);
    }
}
