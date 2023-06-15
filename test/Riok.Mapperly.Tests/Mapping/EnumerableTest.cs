using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

[UsesVerify]
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
            .HaveSingleMethodBody("return source == null ? throw new System.ArgumentNullException(nameof(source)) : source;");
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
                    target[i] = source[i] == null ? throw new System.NullReferenceException($"Sequence {nameof(source)}, contained a null value at index {i}.") : source[i].Value;
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
                var target = new int? [source.Length];
                for (var i = 0; i < source.Length; i++)
                {
                    target[i] = (int? )source[i];
                }

                return target;
                """
            );
    }

    [Fact]
    public void ArrayCustomClassToArrayCustomClass()
    {
        var source = TestSourceBuilder.Mapping("B[]", "B[]", "class B { public int Value {get; set; }}");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source;");
    }

    [Fact]
    public void ArrayCustomClassNullableToArrayCustomClassNonNullable()
    {
        var source = TestSourceBuilder.Mapping("B?[]", "B[]", "class B { public int Value {get; set; }}");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B[source.Length];
                for (var i = 0; i < source.Length; i++)
                {
                    target[i] = source[i] == null ? throw new System.NullReferenceException($"Sequence {nameof(source)}, contained a null value at index {i}.") : source[i];
                }

                return target;
                """
            );
    }

    [Fact]
    public void ArrayCustomClassNonNullableToArrayCustomClassNullable()
    {
        var source = TestSourceBuilder.Mapping("B[]", "B?[]", "class B { public int Value {get; set; }}");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return (global::B? [])source;");
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
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return (string? [])source;");
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
    public void EnumerableToCustomCollection()
    {
        var source = TestSourceBuilder.Mapping("IEnumerable<long>", "B", "class B : ICollection<int> { public void Add(int item) {} }");
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
    public void EnumerableToCustomCollectionWithObjectFactory()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[ObjectFactory] B CreateB() => new();" + "partial B Map(IEnumerable<long> source);",
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
            .GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                return target;
                """
            )
            .HaveDiagnostic(DiagnosticDescriptors.CannotMapToReadOnlyMember);
    }
}
