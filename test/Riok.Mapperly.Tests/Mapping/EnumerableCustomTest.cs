namespace Riok.Mapperly.Tests.Mapping;

public class EnumerableCustomTest
{
    [Fact]
    public void EnumerableToCustomCollection()
    {
        var source = TestSourceBuilder.Mapping(
            "IEnumerable<long>",
            "B",
            TestSourceBuilderOptions.WithAll,
            "class B : ICollection<int> { public void Add(int item) {} }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                foreach (var item in source)
                {
                    target.Add((int)item);
                }
                return target;
                """
            );
    }

    [Fact]
    public void EnumerableToCustomSet()
    {
        var source = TestSourceBuilder.Mapping(
            "IEnumerable<long>",
            "B",
            TestSourceBuilderOptions.WithAll,
            "class B : ISet<int> { public void Add(int item) {} }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                foreach (var item in source)
                {
                    target.Add((int)item);
                }
                return target;
                """
            );
    }

    [Fact]
    public void EnumerableToCustomCollectionWithIgnoredExplicitlyMapped()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty("Count", "MyCount")]
            partial B Map(IReadOnlyCollection<long> source);
            """,
            TestSourceBuilderOptions.WithAll,
            "class B : ICollection<int> { public int MyCount { get; set; } public void Add(int item) {} }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.MyCount = source.Count;
                foreach (var item in source)
                {
                    target.Add((int)item);
                }
                return target;
                """
            );
    }

    [Fact]
    public void EnumerableToCustomCollectionWithEnsureCapacityMethod()
    {
        var source = TestSourceBuilder.Mapping(
            "IEnumerable<long>",
            "B",
            TestSourceBuilderOptions.WithAll,
            "class B : ICollection<int> { public void Add(int item) {} public void EnsureCapacity(int capacity) {} }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                if (global::System.Linq.Enumerable.TryGetNonEnumeratedCount(source, out var sourceCount))
                {
                    target.EnsureCapacity(sourceCount);
                }
                foreach (var item in source)
                {
                    target.Add((int)item);
                }
                return target;
                """
            );
    }

    [Fact]
    public void CustomCollectionToCustomCollection()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            TestSourceBuilderOptions.WithAll,
            "class A : IEnumerable<double> { public int Value { get; set; } }",
            "class B : ICollection<int> { public void Add(int item) {} public int Value { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = source.Value;
                foreach (var item in source)
                {
                    target.Add((int)item);
                }
                return target;
                """
            );
    }

    [Fact]
    public void CollectionToCustomCollectionCapacityConstructor()
    {
        var source = TestSourceBuilder.Mapping(
            "IReadOnlyCollection<string>",
            "B",
            "class B : ICollection<int> { public B(int capacity) {} public void Add(int item) {} }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B(source.Count);
                foreach (var item in source)
                {
                    target.Add(int.Parse(item));
                }
                return target;
                """
            );
    }

    [Fact]
    public void CustomCollectionToCustomCollectionPropertyConstructor()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A : IReadOnlyCollection<string> { public int Value { get; } public int Count { get; } }",
            "class B : ICollection<int> { public B(int value) {} public int Capacity { get; set; } public void Add(int item) {} }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B(source.Value);
                target.Capacity = source.Count;
                foreach (var item in source)
                {
                    target.Add(int.Parse(item));
                }
                return target;
                """
            );
    }

    [Fact]
    public void EnumerableToCustomCollectionWithCapacityMember()
    {
        var source = TestSourceBuilder.Mapping(
            "IEnumerable<int>",
            "A",
            "class A : ICollection<int> { public int Capacity { get; set; } public void Add(int item) {} }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::A();
                if (global::System.Linq.Enumerable.TryGetNonEnumeratedCount(source, out var sourceCount))
                {
                    target.Capacity = sourceCount;
                }
                foreach (var item in source)
                {
                    target.Add(item);
                }
                return target;
                """
            );
    }

    [Fact]
    public void EnumerableToCustomCollectionWithReadOnlyCapacityMember()
    {
        var source = TestSourceBuilder.Mapping(
            "IEnumerable<int>",
            "A",
            "class A : ICollection<int> { public int Capacity { get; } public void Add(int item) {} }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::A();
                foreach (var item in source)
                {
                    target.Add(item);
                }
                return target;
                """
            );
    }

    [Fact]
    public void CustomCollectionToCustomCollectionPropertyAndCapacityConstructor()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A : IReadOnlyCollection<string> { public int Value { get; } public int Count { get; } }",
            "class B : ICollection<int> { public B(int value, int capacity) {} public void EnsureCapacity(int count) {} public void Add(int item) {} }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B(source.Value, source.Count);
                foreach (var item in source)
                {
                    target.Add(int.Parse(item));
                }
                return target;
                """
            );
    }

    [Fact]
    public void ArrayToCustomCollectionCountConstructor()
    {
        var source = TestSourceBuilder.Mapping(
            "string[]",
            "B",
            "class B : ICollection<int> { public B(int count) {} public void Add(int item) {} }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B(source.Length);
                foreach (var item in source)
                {
                    target.Add(int.Parse(item));
                }
                return target;
                """
            );
    }

    [Fact]
    public void CustomCollectionToExistingCustomCollection()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial void Map(A source, B target);",
            TestSourceBuilderOptions.WithAll,
            "class A : IEnumerable<double> { public int Value { get; set; } }",
            "class B : ICollection<int> { public void Add(int item) {} public int Value { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                target.Value = source.Value;
                foreach (var item in source)
                {
                    target.Add((int)item);
                }
                """
            );
    }

    [Fact]
    public void CustomCollectionToCustomCollectionWithMapProperty()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """[MapProperty("ValueSource", "ValueTarget")] public partial B Map(A source);""",
            TestSourceBuilderOptions.WithAll,
            "class A : IEnumerable<double> { public int ValueSource { get; set; } }",
            "class B : ICollection<int> { public void Add(int item) {} public int ValueTarget { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.ValueTarget = source.ValueSource;
                foreach (var item in source)
                {
                    target.Add((int)item);
                }
                return target;
                """
            );
    }

    [Fact]
    public void EnumerableToExplicitAddCustomCollection()
    {
        // should not create a mapping using a looping add method inside a foreach loop when the add method is explicit
        var source = TestSourceBuilder.Mapping(
            "IEnumerable<int>",
            "B",
            "class B : ICollection<int> { void ICollection<int>.Add(int item) {} }"
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

    [Fact]
    public void EnumerableToCustomCollectionWithObjectFactory()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [ObjectFactory] B CreateB() => new();
            partial B Map(IEnumerable<long> source);
            """,
            TestSourceBuilderOptions.WithAll,
            "class B : ICollection<int> { public void Add(int item) {} }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = CreateB();
                foreach (var item in source)
                {
                    target.Add((int)item);
                }
                return target;
                """
            );
    }

    [Fact]
    public void CustomCollectionToCustomCollectionWithObjectFactory()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [ObjectFactory] B CreateB() => new();
            partial B Map(A source);
            """,
            "class A : ICollection<int> { public int Value { get; } public void Add(int item) {} }",
            "class B : ICollection<int> { public int Value { set; } public void Add(int item) {} }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = CreateB();
                target.Value = source.Value;
                foreach (var item in source)
                {
                    target.Add(item);
                }
                return target;
                """
            );
    }

    [Fact]
    public void CustomCollectionToGetOnlyICollection()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public C Value { get; } }",
            "class B { public ICollection<int> Value { get; } }",
            "class C : IEnumerable<int> { public int Capacity { get; } public int Count { get; } }"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowAndIncludeAllDiagnostics)
            .Should()
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                foreach (var item in source.Value)
                {
                    target.Value.Add(item);
                }
                return target;
                """
            );
    }

    [Fact]
    public void CustomCollectionToGetOnlyList()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public C Value { get; } }",
            "class B { public List<int> Value { get; } }",
            "class C : IEnumerable<int> { public int Capacity { get; } public int Count { get; } }"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowAndIncludeAllDiagnostics)
            .Should()
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value.EnsureCapacity(source.Value.Count + target.Value.Count);
                foreach (var item in source.Value)
                {
                    target.Value.Add(item);
                }
                return target;
                """
            );
    }
}
