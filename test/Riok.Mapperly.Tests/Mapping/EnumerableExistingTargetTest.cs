using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Tests.Mapping;

public class EnumerableExistingTargetTest
{
    [Fact]
    public void MapToExistingExplicitAddCustomCollection()
    {
        // should not create a mapping using a looped add inside a foreach loop when th add method is explicit
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial void Map(IEnumerable<int> source, B target);",
            "class B : ICollection<int> { void ICollection<int>.Add(int item) {} }"
        );
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("");
    }

    [Fact]
    public void EnumerableToExistingCustomCollection()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial void Map(IEnumerable<int> source, A target);",
            "class A : List<int> { }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                if (global::System.Linq.Enumerable.TryGetNonEnumeratedCount(source, out var sourceCount))
                {
                    target.EnsureCapacity(sourceCount + target.Count);
                }
                foreach (var item in source)
                {
                    target.Add(item);
                }
                """
            );
    }

    [Fact]
    public Task MapToExistingCollectionShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "private partial void Map(List<A>? source, RepeatedField<B> target);",
            "class RepeatedField<T> : IList<T> { public void Add(T item) {} }",
            "class A { public string Value { get; set; } }",
            "class B { public string Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void MapToExistingCollectionWithUnmappableElementsShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial void Map(IEnumerable<A> source, List<int> target);",
            "class A { public string Value { get; set; } }"
        );

        TestHelper.GenerateMapper(source, TestHelperOptions.AllowDiagnostics).Should().HaveSingleMethodBody(string.Empty);
    }

    [Fact]
    public Task MapToExistingStackShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "private partial void Map(List<A>? source, Stack<B> target);",
            "class A { public string Value { get; set; } }",
            "class B { public string Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task MapToExistingQueueShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "private partial void Map(List<A>? source, Queue<B> target);",
            "class A { public string Value { get; set; } }",
            "class B { public string Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void EnumerableMappingExistingTargetDisabledShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            TestSourceBuilderOptions.WithDisabledMappingConversion(MappingConversionType.Enumerable),
            "class A { public IEnumerable<int> Value { get; } }",
            "class B { public IReadOnlyCollection<int> Value { get; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                return target;
                """
            );
    }
}
