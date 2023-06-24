using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Tests.Helpers;

public class EnumerableExtensionsTest
{
    [Fact]
    public void WhereValueTypeNotNullShouldFilterNulls()
    {
        new int?[] { 1, null, null, 2, 3, 4, null }
            .WhereNotNull()
            .Should()
            .BeEquivalentTo(new[] { 1, 2, 3, 4 }, o => o.WithStrictOrdering());
    }

    [Fact]
    public void WhereNotNullShouldFilterNulls()
    {
        new[] { "a", "b", "c", null, "d", null, null, "e" }
            .WhereNotNull()
            .Should()
            .BeEquivalentTo(new[] { "a", "b", "c", "d", "e" }, o => o.WithStrictOrdering());
    }

    [Fact]
    public void ToHashSetShouldWork()
    {
        var items = new[] { 1, 1, 2, 3, 4, 5, 5 };

        // can't use extension method due to ambiguous method reference.
        // (no support for this method in netstandard2.0)
        var hashSet = EnumerableExtensions.ToHashSet(items);
        hashSet.Should().BeEquivalentTo(new[] { 1, 2, 3, 4, 5 });
    }

    [Fact]
    public void HashSetAddRangeShouldWork()
    {
        var items = new[] { 1, 1, 2, 3, 4, 5, 5 };
        var hashSet = new HashSet<int>() { 1, 3, 6 };

        hashSet.AddRange(items);

        hashSet.Should().BeEquivalentTo(new[] { 1, 2, 3, 4, 5, 6 });
    }

    [Fact]
    public void DistinctByShouldWork()
    {
        var items = new[] { ("item10", 10), ("item11", 10), ("item12", 10), ("item20", 20), ("item30", 30), ("item31", 30), };

        items
            .DistinctBy(x => x.Item2)
            .Select(x => x.Item1)
            .Should()
            .BeEquivalentTo(new[] { "item10", "item20", "item30" }, o => o.WithStrictOrdering());
    }

    [Fact]
    public void SkipLastShouldWork()
    {
        var items = new[] { 1, 2, 5, 6, 7 };
        items.SkipLast().Should().BeEquivalentTo(items.Take(items.Length - 1), o => o.WithoutStrictOrdering());
    }

    [Fact]
    public void AggregateWithPrevious()
    {
        var items = new[] { 1, 2, 5, 6, 7 };
        items.AggregateWithPrevious(100, (agg, prev, item) => agg - prev + item).Should().Be(107);
    }
}
