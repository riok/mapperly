namespace Riok.Mapperly.Tests.Mapping;

public class QueryableProjectionEnumerableTest
{
    [Fact]
    public Task ExplicitCast()
    {
        var source = TestSourceBuilder.Mapping(
            "System.Linq.IQueryable<A>",
            "System.Linq.IQueryable<B>",
            TestSourceBuilderOptions.AllConversions,
            "class A { public IEnumerable<long> Values { get; set; } }",
            "class B { public IEnumerable<int> Values { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ArrayToArrayExplicitCast()
    {
        var source = TestSourceBuilder.Mapping(
            "System.Linq.IQueryable<A>",
            "System.Linq.IQueryable<B>",
            TestSourceBuilderOptions.AllConversions,
            "class A { public long[] Values { get; set; } }",
            "class B { public int[] Values { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }
}
