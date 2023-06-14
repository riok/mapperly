namespace Riok.Mapperly.Tests.Mapping;

public class EnumerableSetTest
{
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
