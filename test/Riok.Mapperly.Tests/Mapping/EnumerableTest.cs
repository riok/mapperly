using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class EnumerableTest
{
    [Fact]
    public void ArrayToArrayOfPrimitiveTypes()
    {
        var source = TestSourceBuilder.Mapping("int[]", "int[]");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source;");
    }

    [Fact]
    public void NullableArrayToNonNullableArray()
    {
        var source = TestSourceBuilder.Mapping("int[]?", "int[]");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return source ?? throw new System.ArgumentNullException(nameof(source));");
    }

    [Fact]
    public void ArrayOfNullablePrimitiveTypesToNonNullableArray()
    {
        var source = TestSourceBuilder.Mapping("int?[]", "int[]");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new int[source.Length];
                for (var i = 0; i < source.Length; i++)
                {
                    target[i] = source[i] ?? throw new System.NullReferenceException($"Sequence {nameof(source)}, contained a null value at index {i}.");
                }
                return target;
                """
            );
    }

    [Fact]
    public void ArrayOfPrimitiveTypesToNullablePrimitiveTypesArray()
    {
        var source = TestSourceBuilder.Mapping("int[]", "int?[]");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new int?[source.Length];
                for (var i = 0; i < source.Length; i++)
                {
                    target[i] = (int?)source[i];
                }
                return target;
                """
            );
    }

    [Fact]
    public void ArrayCustomClassToArrayCustomClass()
    {
        var source = TestSourceBuilder.Mapping("B[]", "B[]", "class B { public int Value { get; set; } }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source;");
    }

    [Fact]
    public void ArrayCustomClassNullableToArrayCustomClassNonNullable()
    {
        var source = TestSourceBuilder.Mapping("B?[]", "B[]", "class B { public int Value { get; set; } }");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B[source.Length];
                for (var i = 0; i < source.Length; i++)
                {
                    target[i] = source[i] ?? throw new System.NullReferenceException($"Sequence {nameof(source)}, contained a null value at index {i}.");
                }
                return target;
                """
            );
    }

    [Fact]
    public void ArrayCustomClassNonNullableToArrayCustomClassNullable()
    {
        var source = TestSourceBuilder.Mapping("B[]", "B?[]", "class B { public int Value { get; set; } }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return (global::B?[])source;");
    }

    [Fact]
    public void ArrayToArrayOfString()
    {
        var source = TestSourceBuilder.Mapping("string[]", "string[]");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source;");
    }

    [Fact]
    public void ArrayToArrayOfNullableString()
    {
        var source = TestSourceBuilder.Mapping("string[]", "string?[]");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return (string?[])source;");
    }

    [Fact]
    public void ArrayToArrayOfReadOnlyStruct()
    {
        var source = TestSourceBuilder.Mapping("A[]", "A[]", "readonly struct A{}");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source;");
    }

    [Fact]
    public void ArrayToArrayOfMutableStruct()
    {
        var source = TestSourceBuilder.Mapping("A[]", "A[]", "struct A{}");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source;");
    }

    [Fact]
    public void ArrayToArrayOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping("long[]", "int[]");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new int[source.Length];
                for (var i = 0; i < source.Length; i++)
                {
                    target[i] = (int)source[i];
                }
                return target;
                """
            );
    }

    [Fact]
    public void EnumerableToArrayOfPrimitiveTypes()
    {
        var source = TestSourceBuilder.Mapping("IEnumerable<int>", "int[]");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::System.Linq.Enumerable.ToArray(source);");
    }

    [Fact]
    public void EnumerableToEnumerableOfPrimitiveTypes()
    {
        var source = TestSourceBuilder.Mapping("IEnumerable<int>", "IEnumerable<int>");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source;");
    }

    [Fact]
    public void EnumerableToICollectionOfPrimitiveTypes()
    {
        var source = TestSourceBuilder.Mapping("IEnumerable<int>", "ICollection<int>");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::System.Linq.Enumerable.ToList(source);");
    }

    [Fact]
    public void EnumerableToReadOnlyCollectionOfPrimitiveTypes()
    {
        var source = TestSourceBuilder.Mapping("IEnumerable<int>", "IReadOnlyCollection<int>");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::System.Linq.Enumerable.ToList(source);");
    }

    [Fact]
    public void CollectionToReadOnlyCollectionOfPrimitiveTypes()
    {
        var source = TestSourceBuilder.Mapping("ICollection<int>", "IReadOnlyCollection<int>");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::System.Linq.Enumerable.ToArray(source);");
    }

    [Fact]
    public void ReadOnlyCollectionToCollectionOfPrimitiveTypes()
    {
        var source = TestSourceBuilder.Mapping("IReadOnlyCollection<int>", "ICollection<int>");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::System.Linq.Enumerable.ToList(source);");
    }

    [Fact]
    public void ReadOnlyCollectionToIListOfDifferentTypes()
    {
        var source = TestSourceBuilder.Mapping("IReadOnlyCollection<int>", "IList<long>");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::System.Collections.Generic.List<long>(source.Count);
                foreach (var item in source)
                {
                    target.Add((long)item);
                }
                return target;
                """
            );
    }

    [Fact]
    public void ReadOnlyCollectionToArrayOfDifferentTypes()
    {
        var source = TestSourceBuilder.Mapping("IReadOnlyCollection<int>", "string[]");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new string[source.Count];
                var i = 0;
                foreach (var item in source)
                {
                    target[i] = item.ToString();
                    i++;
                }
                return target;
                """
            );
    }

    [Fact]
    public void EnumerableToReadOnlyCollectionOfImplicitTypes()
    {
        var source = TestSourceBuilder.Mapping("IEnumerable<int>", "IReadOnlyCollection<long>");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                "return global::System.Linq.Enumerable.ToList(global::System.Linq.Enumerable.Select(source, x => (long)x));"
            );
    }

    [Fact]
    public void EnumerableToReadOnlyCollectionOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping("IEnumerable<long>", "IReadOnlyCollection<int>");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                "return global::System.Linq.Enumerable.ToList(global::System.Linq.Enumerable.Select(source, x => (int)x));"
            );
    }

    [Fact]
    public void ListToArrayOfPrimitiveTypes()
    {
        var source = TestSourceBuilder.Mapping("List<int>", "int[]");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::System.Linq.Enumerable.ToArray(source);");
    }

    [Fact]
    public void ListToArrayOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping("List<int>", "long[]");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new long[source.Count];
                var i = 0;
                foreach (var item in source)
                {
                    target[i] = (long)item;
                    i++;
                }
                return target;
                """
            );
    }

    [Fact]
    public void EnumerableToIListOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping("IEnumerable<long>", "IList<int>");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                "return global::System.Linq.Enumerable.ToList(global::System.Linq.Enumerable.Select(source, x => (int)x));"
            );
    }

    [Fact]
    public void EnumerableToListOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping("IEnumerable<long>", "List<int>");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                "return global::System.Linq.Enumerable.ToList(global::System.Linq.Enumerable.Select(source, x => (int)x));"
            );
    }

    [Fact]
    public void EnumerableToIReadOnlyListOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping("IEnumerable<long>", "IReadOnlyList<int>");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                "return global::System.Linq.Enumerable.ToList(global::System.Linq.Enumerable.Select(source, x => (int)x));"
            );
    }

    [Fact]
    public void ReadOnlyCollectionToList()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IReadOnlyCollection<int> Value { get; } }",
            "class B { public List<int> Value { get; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
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

    [Fact]
    public void EnumerableToStack()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IEnumerable<int> Value { get; } }",
            "class B { public Stack<long> Value { get; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                if (global::System.Linq.Enumerable.TryGetNonEnumeratedCount(source.Value, out var sourceCount))
                {
                    target.Value.EnsureCapacity(sourceCount + target.Value.Count);
                }
                foreach (var item in source.Value)
                {
                    target.Value.Push((long)item);
                }
                return target;
                """
            );
    }

    [Fact]
    public void EnumerableToQueue()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IEnumerable<int> Value { get; } }",
            "class B { public Queue<long> Value { get; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                if (global::System.Linq.Enumerable.TryGetNonEnumeratedCount(source.Value, out var sourceCount))
                {
                    target.Value.EnsureCapacity(sourceCount + target.Value.Count);
                }
                foreach (var item in source.Value)
                {
                    target.Value.Enqueue((long)item);
                }
                return target;
                """
            );
    }

    [Fact]
    public void EnumerableToCreatedStack()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IEnumerable<int> Value { get; } }",
            "class B { public Stack<int> Value { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = new global::System.Collections.Generic.Stack<int>(source.Value);
                return target;
                """
            );
    }

    [Fact]
    public void EnumerableToCreatedQueue()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IEnumerable<int> Value { get; } }",
            "class B { public Queue<int> Value { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = new global::System.Collections.Generic.Queue<int>(source.Value);
                return target;
                """
            );
    }

    [Fact]
    public void EnumerableToCreatedStackOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IEnumerable<int> Value { get; } }",
            "class B { public Stack<long> Value { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = new global::System.Collections.Generic.Stack<long>(global::System.Linq.Enumerable.Select(source.Value, x => (long)x));
                return target;
                """
            );
    }

    [Fact]
    public void EnumerableToCreatedQueueOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IEnumerable<int> Value { get; } }",
            "class B { public Queue<long> Value { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = new global::System.Collections.Generic.Queue<long>(global::System.Linq.Enumerable.Select(source.Value, x => (long)x));
                return target;
                """
            );
    }

    [Fact]
    public Task ArrayToReadOnlyCollectionShouldUpgradeNullability()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public int[] Value { get; set;} }",
            "class B { public IReadOnlyCollection<string> Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source, TestHelperOptions.DisabledNullable);
    }

    [Fact]
    public Task ShouldUpgradeNullabilityOfGenericInDisabledNullableContext()
    {
        var source = TestSourceBuilder.Mapping("IList<A>", "IList<B>", "record A(int V);", "record B(int V);");
        return TestHelper.VerifyGenerator(source, TestHelperOptions.DisabledNullable);
    }

    [Fact]
    public Task ArrayToCollectionShouldUpgradeNullability()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public int[] Value { get; set;} }",
            "class B { public ICollection<string> Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source, TestHelperOptions.DisabledNullable);
    }

    [Fact]
    public Task ArrayToList()
    {
        var source = TestSourceBuilder.Mapping("int[]", "List<string>");

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ArrayToListShouldUpgradeNullability()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public int[] Value { get; set;} }",
            "class B { public List<string> Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source, TestHelperOptions.DisabledNullable);
    }

    [Fact]
    public Task CollectionToReadOnlyCollectionShouldUpgradeNullability()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public ICollection<int> Value { get; set;} }",
            "class B { public IReadOnlyCollection<string> Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source, TestHelperOptions.DisabledNullable);
    }

    [Fact]
    public Task ShouldUpgradeNullabilityInDisabledNullableContextInSelectClause()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public C[] Value { get; set;} }",
            "class B { public D[] Value { get; set; } }",
            "class C { public string Value { get; set; } }",
            "class D { public string Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source, TestHelperOptions.DisabledNullable);
    }

    [Fact]
    public Task MapToReadOnlyNullableCollectionProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public ICollection<int> Value { get; } }",
            "class B { public ICollection<long>? Value { get; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task MapToReadOnlyNullableICollectionProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public ICollection<int> Value { get; } }",
            "class B { public ICollection<long>? Value { get; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task MapToReadOnlyNullableCollectionPropertyFromNullable()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public ICollection<int>? Value { get; } }",
            "class B { public ICollection<long>? Value { get; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task MapToReadOnlyCollectionPropertyFromNullable()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public ICollection<int>? Value { get; } }",
            "class B { public ICollection<long> Value { get; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task MapToReadOnlyCollectionProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public ICollection<int> Value { get; } }",
            "class B { public ICollection<long> Value { get; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void EnumerableMappingDisabledShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "IEnumerable<long>",
            "IEnumerable<int>",
            TestSourceBuilderOptions.WithDisabledMappingConversion(MappingConversionType.Enumerable)
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.CouldNotCreateMapping)
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void EnumerableToReadOnlyArrayPropertyShouldDiagnostic()
    {
        // should not create a mapping that maps to an array by adding to it in a foreach loop
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public ICollection<int> Value { get; } }",
            "class B { public int[] Value { get; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.CannotMapToReadOnlyType, "Cannot map to read-only type int[]")
            .HaveAssertedAllDiagnostics()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                return target;
                """
            );
    }

    [Fact]
    public Task EnumerableShouldReuseForReadOnlyCollectionImplementorsButDifferentForICollection()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public partial BList Map(AList source);
            public partial BListAgain Map(AListAgain source);
            public partial BReadOnlyCollection Map(AList source);
            public partial BCustomCollection Map(AList source);
            public partial BCollection Map(AList source);g
            public partial BReadOnlyCollection Map(AReadOnlyCollection source);
            public partial BCustomCollection Map(ACustomReadOnlyCollection source);
            public partial BCollection Map(ACollection source);
            public partial BCustomCollection Map(ACustomCollection source);
            """,
            "record AList(List<C> Values);",
            "record AListAgain(List<C> Values);",
            "record AReadOnlyCollection(IReadOnlyCollection<C> Values);",
            "record ACustomReadOnlyCollection(CustomReadOnlyCollection<C> Values);",
            "record ACollection(ICollection<C> Values);",
            "record ACustomCollection(CustomCollection<C> Values);",
            "record BList(List<D> Values);",
            "record BListAgain(List<D> Values);",
            "record BReadOnlyCollection(IReadOnlyCollection<D> Values);",
            "record BCollection(ICollection<D> Values);",
            "record BCustomCollection(CustomCollection<D> Values);",
            "public class CustomReadOnlyCollection<T> : IReadOnlyCollection<T> { public void Add(T item) {} }",
            "public class CustomCollection<T> : ICollection<T> { public void Add(T item) {} }",
            "record C(int Value);",
            "record D(int Value);"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task NestedInterfacedLists()
    {
        var source = TestSourceBuilder.Mapping(
            "IReadOnlyList<IReadOnlyCollection<IReadOnlyList<int>>>",
            "IReadOnlyList<IReadOnlyCollection<IReadOnlyList<string>>>"
        );

        return TestHelper.VerifyGenerator(source);
    }
}
