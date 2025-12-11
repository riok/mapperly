namespace Riok.Mapperly.Tests.Mapping;

public class EnumerableSetTest
{
    [Fact]
    public void EnumerableToReadOnlySet()
    {
        var source = TestSourceBuilder.Mapping("IEnumerable<long>", "IReadOnlySet<int>", TestSourceBuilderOptions.AllConversions);
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return global::System.Linq.Enumerable.ToHashSet(global::System.Linq.Enumerable.Select(source, x => (int)x));
                """
            );
    }

    [Fact]
    public void EnumerableToSet()
    {
        var source = TestSourceBuilder.Mapping("IEnumerable<long>", "ISet<int>", TestSourceBuilderOptions.AllConversions);
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return global::System.Linq.Enumerable.ToHashSet(global::System.Linq.Enumerable.Select(source, x => (int)x));
                """
            );
    }

    [Fact]
    public void EnumerableToHashSet()
    {
        var source = TestSourceBuilder.Mapping("IEnumerable<long>", "HashSet<int>", TestSourceBuilderOptions.AllConversions);
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return global::System.Linq.Enumerable.ToHashSet(global::System.Linq.Enumerable.Select(source, x => (int)x));
                """
            );
    }

    [Fact]
    public void EnumerableToSortedSet()
    {
        var source = TestSourceBuilder.Mapping("IEnumerable<long>", "SortedSet<int>", TestSourceBuilderOptions.AllConversions);
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return new global::System.Collections.Generic.SortedSet<int>(global::System.Linq.Enumerable.Select(source, x => (int)x));
                """
            );
    }

    [Fact]
    public void ExistingEnumerableToExistingSet()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IEnumerable<int> Values { get; } }",
            "class B { public ISet<int> Values { get; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                foreach (var item in source.Values)
                {
                    target.Values.Add(item);
                }
                return target;
                """
            );
    }

    [Fact]
    public void ExistingEnumerableToExistingHashSet()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IEnumerable<int> Values { get; } }",
            "class B { public HashSet<int> Values { get; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                if (global::System.Linq.Enumerable.TryGetNonEnumeratedCount(source.Values, out var sourceCount))
                {
                    target.Values.EnsureCapacity(sourceCount + target.Values.Count);
                }
                foreach (var item in source.Values)
                {
                    target.Values.Add(item);
                }
                return target;
                """
            );
    }
}
