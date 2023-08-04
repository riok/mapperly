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
