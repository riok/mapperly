using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Tests.Helpers;

public class EnumerableExtensionsTest
{
    [Fact]
    public void WhereValueTypeNotNullShouldFilterNulls()
    {
        new int?[] { 1, null, null, 2, 3, 4, null }
            .WhereNotNull()
            .ShouldBe([1, 2, 3, 4]);
    }

    [Fact]
    public void WhereNotNullShouldFilterNulls()
    {
        new[] { "a", "b", "c", null, "d", null, null, "e" }.WhereNotNull().ShouldBe(["a", "b", "c", "d", "e"]);
    }

    [Fact]
    public void SkipLastShouldWork()
    {
        var items = new[] { 1, 2, 5, 6, 7 };
        items.SkipLast().ShouldBe(items.Take(items.Length - 1));
    }

    [Fact]
    public void AggregateWithPrevious()
    {
        var items = new[] { 1, 2, 5, 6, 7 };
        items.AggregateWithPrevious(100, (agg, prev, item) => agg - prev + item).ShouldBe(107);
    }
}
