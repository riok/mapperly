using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class EnumerableDeepCloningTest
{
    [Fact]
    public void ArrayOfPrimitivesToReadOnlyCollectionDeepCloning()
    {
        var source = TestSourceBuilder.Mapping("int[]", "IReadOnlyCollection<int>", TestSourceBuilderOptions.WithDeepCloning);
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return (global::System.Collections.Generic.IReadOnlyCollection<int>)source.Clone();");
    }

    [Fact]
    public void ArrayOfPrimitivesToEnumerableDeepCloning()
    {
        var source = TestSourceBuilder.Mapping("int[]", "IEnumerable<int>", TestSourceBuilderOptions.WithDeepCloning);
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return (global::System.Collections.Generic.IEnumerable<int>)source.Clone();");
    }

    [Fact]
    public void ArrayToArrayOfPrimitiveTypesDeepCloning()
    {
        var source = TestSourceBuilder.Mapping("int[]", "int[]", TestSourceBuilderOptions.WithDeepCloning);
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return (int[])source.Clone();");
    }

    [Fact]
    public void ArrayOfNullablePrimitiveTypesToNonNullableArrayDeepCloning()
    {
        var source = TestSourceBuilder.Mapping("int?[]", "int[]", TestSourceBuilderOptions.WithDeepCloning);
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.NullableSourceTypeToNonNullableTargetType,
                "Mapping the nullable source of type int? to target of type int which is not nullable"
            )
            .HaveAssertedAllDiagnostics()
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
    public void ArrayOfPrimitiveTypesToNullablePrimitiveTypesArrayDeepCloning()
    {
        var source = TestSourceBuilder.Mapping("int[]", "int?[]", TestSourceBuilderOptions.WithDeepCloning);
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
    public void ArrayCustomClassToArrayCustomClassDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "B[]",
            "B[]",
            TestSourceBuilderOptions.WithDeepCloning,
            "class B { public int Value { get; set; }}"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B[source.Length];
                for (var i = 0; i < source.Length; i++)
                {
                    target[i] = MapToB(source[i]);
                }
                return target;
                """
            );
    }

    [Fact]
    public void ArrayToArrayOfStringDeepCloning()
    {
        var source = TestSourceBuilder.Mapping("string[]", "string[]", TestSourceBuilderOptions.WithDeepCloning);
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return (string[])source.Clone();");
    }

    [Fact]
    public void ArrayToArrayOfNullableStringDeepCloning()
    {
        var source = TestSourceBuilder.Mapping("string[]", "string?[]", TestSourceBuilderOptions.WithDeepCloning);
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return (string?[])source.Clone();");
    }

    [Fact]
    public void ArrayToArrayOfReadOnlyStructDeepCloning()
    {
        var source = TestSourceBuilder.Mapping("A[]", "A[]", TestSourceBuilderOptions.WithDeepCloning, "readonly struct A{}");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return (global::A[])source.Clone();");
    }

    [Fact]
    public void ArrayToArrayOfMutableStructDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "A[]",
            "A[]",
            TestSourceBuilderOptions.WithDeepCloning,
            "struct A{ public string Value { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::A[source.Length];
                for (var i = 0; i < source.Length; i++)
                {
                    target[i] = MapToA(source[i]);
                }
                return target;
                """
            );
    }

    [Fact]
    public void ArrayToArrayOfUnmanagedStructDeepCloning()
    {
        var source = TestSourceBuilder.Mapping("A[]", "A[]", TestSourceBuilderOptions.WithDeepCloning, "struct A{}");
        TestHelper.GenerateMapper(source).Should().HaveMapMethodBody("return (global::A[])source.Clone();");
    }

    [Fact]
    public void CollectionToArrayOfMutableStructDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "ICollection<A>",
            "A[]",
            TestSourceBuilderOptions.WithDeepCloning,
            "struct A { public string Value { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::A[source.Count];
                var i = 0;
                foreach (var item in source)
                {
                    target[i] = MapToA(item);
                    i++;
                }
                return target;
                """
            );
    }

    [Fact]
    public void CollectionToArrayOfUnmanagedStructDeepCloning()
    {
        var source = TestSourceBuilder.Mapping("ICollection<A>", "A[]", TestSourceBuilderOptions.WithDeepCloning, "struct A{}");
        TestHelper.GenerateMapper(source).Should().HaveMapMethodBody("return global::System.Linq.Enumerable.ToArray(source);");
    }

    [Fact]
    public void ArrayToArrayOfMutableStructDeepCloningLoopNameTaken()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial A[] Map(A[] i);",
            TestSourceBuilderOptions.WithDeepCloning,
            "struct A{ public string Value { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::A[i.Length];
                for (var i1 = 0; i1 < i.Length; i1++)
                {
                    target[i1] = MapToA(i[i1]);
                }
                return target;
                """
            );
    }

    [Fact]
    public void ArrayToArrayOfMutableStructDeepCloningTargetNameTaken()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial A[] Map(A[] target);",
            TestSourceBuilderOptions.WithDeepCloning,
            "struct A{ public string Value { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target1 = new global::A[target.Length];
                for (var i = 0; i < target.Length; i++)
                {
                    target1[i] = MapToA(target[i]);
                }
                return target1;
                """
            );
    }

    [Fact]
    public void EnumerableOfPrimitivesToEnumerableDeepCloning()
    {
        var source = TestSourceBuilder.Mapping("IEnumerable<int>", "IEnumerable<int>", TestSourceBuilderOptions.WithDeepCloning);
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::System.Linq.Enumerable.ToList(source);");
    }

    [Fact]
    public void ReadOnlyCollectionOfPrimitivesToEnumerableDeepCloning()
    {
        var source = TestSourceBuilder.Mapping("IReadOnlyCollection<int>", "IEnumerable<int>", TestSourceBuilderOptions.WithDeepCloning);
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::System.Linq.Enumerable.ToArray(source);");
    }

    [Fact]
    public void EnumerableOfPrimitivesToReadOnlyCollectionDeepCloning()
    {
        var source = TestSourceBuilder.Mapping("IEnumerable<int>", "IReadOnlyCollection<int>", TestSourceBuilderOptions.WithDeepCloning);
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::System.Linq.Enumerable.ToList(source);");
    }
}
