using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Tests.Helpers;

public class ListDictionaryTests
{
    [Fact]
    public void AddAndGetOrEmptyShouldAddValueAndReturn()
    {
        var l = new ListDictionary<int, int>();
        l.Add(1, 1);
        l.GetOrEmpty(1).ShouldBe([1]);
        l.Add(1, 2);
        l.GetOrEmpty(1).ShouldBe([1, 2]);
        l.Add(2, 3);
        l.GetOrEmpty(1).ShouldBe([1, 2]);
        l.GetOrEmpty(2).ShouldBe([3]);
    }

    [Fact]
    public void GetOrEmptyForEmptyShouldReturnEmpty()
    {
        var l = new ListDictionary<int, int>();
        l.GetOrEmpty(1).ShouldBeEmpty();
    }

    [Fact]
    public void ContainsKey()
    {
        var l = new ListDictionary<int, int>();
        l.ContainsKey(1).ShouldBeFalse();
        l.Add(1, 1);
        l.ContainsKey(1).ShouldBeTrue();
    }

    [Fact]
    public void Remove()
    {
        var l = new ListDictionary<int, int>();
        l.Add(1, 1);
        l.ContainsKey(1).ShouldBeTrue();
        l.Remove(1);
        l.ContainsKey(1).ShouldBeFalse();
    }

    [Fact]
    public void RemoveUnknown()
    {
        var l = new ListDictionary<int, int>();
        l.Remove(1);
    }
}
