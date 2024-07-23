using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class SpanTest
{
    [Fact]
    public void SpanToSpan()
    {
        var source = TestSourceBuilder.Mapping("Span<int>", "Span<int>");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source;");
    }

    [Fact]
    public void SpanToReadOnlySpan()
    {
        var source = TestSourceBuilder.Mapping("Span<int>", "ReadOnlySpan<int>");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return (global::System.ReadOnlySpan<int>)source;");
    }

    [Fact]
    public void ReadOnlySpanToReadOnlySpan()
    {
        var source = TestSourceBuilder.Mapping("ReadOnlySpan<int>", "ReadOnlySpan<int>");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source;");
    }

    [Fact]
    public void ReadOnlySpanToSpan()
    {
        var source = TestSourceBuilder.Mapping("ReadOnlySpan<int>", "Span<int>");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source.ToArray();");
    }

    [Fact]
    public void SpanToSpanOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping("Span<int>", "Span<long>");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new long[source.Length];
                for (var i = 0; i < source.Length; i++)
                {
                    target[i] = (long)source[i];
                }
                return target;
                """
            );
    }

    [Fact]
    public void SpanToReadOnlySpanOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping("Span<int>", "ReadOnlySpan<long>");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new long[source.Length];
                for (var i = 0; i < source.Length; i++)
                {
                    target[i] = (long)source[i];
                }
                return target;
                """
            );
    }

    [Fact]
    public void ReadOnlySpanToReadOnlySpanOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping("ReadOnlySpan<int>", "ReadOnlySpan<long>");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new long[source.Length];
                for (var i = 0; i < source.Length; i++)
                {
                    target[i] = (long)source[i];
                }
                return target;
                """
            );
    }

    [Fact]
    public void ReadOnlySpanToSpanOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping("ReadOnlySpan<int>", "Span<long>");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new long[source.Length];
                for (var i = 0; i < source.Length; i++)
                {
                    target[i] = (long)source[i];
                }
                return target;
                """
            );
    }

    [Fact]
    public void ArrayToSpan()
    {
        var source = TestSourceBuilder.Mapping("int[]", "Span<int>");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return (global::System.Span<int>)source;");
    }

    [Fact]
    public void ArrayToReadOnlySpan()
    {
        var source = TestSourceBuilder.Mapping("int[]", "ReadOnlySpan<int>");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return (global::System.ReadOnlySpan<int>)source;");
    }

    [Fact]
    public void SpanToArray()
    {
        var source = TestSourceBuilder.Mapping("Span<int>", "int[]");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source.ToArray();");
    }

    [Fact]
    public void ReadOnlySpanToArray()
    {
        var source = TestSourceBuilder.Mapping("ReadOnlySpan<int>", "int[]");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source.ToArray();");
    }

    [Fact]
    public void ArrayToSpanOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping("int[]", "Span<long>");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new long[source.Length];
                for (var i = 0; i < source.Length; i++)
                {
                    target[i] = (long)source[i];
                }
                return target;
                """
            );
    }

    [Fact]
    public void ArrayToReadOnlySpanOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping("int[]", "ReadOnlySpan<long>");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new long[source.Length];
                for (var i = 0; i < source.Length; i++)
                {
                    target[i] = (long)source[i];
                }
                return target;
                """
            );
    }

    [Fact]
    public void SpanToArrayOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping("Span<int>", "long[]");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new long[source.Length];
                for (var i = 0; i < source.Length; i++)
                {
                    target[i] = (long)source[i];
                }
                return target;
                """
            );
    }

    [Fact]
    public void ReadOnlySpanToArrayOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping("ReadOnlySpan<int>", "long[]");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new long[source.Length];
                for (var i = 0; i < source.Length; i++)
                {
                    target[i] = (long)source[i];
                }
                return target;
                """
            );
    }

    [Fact]
    public void EnumerableToSpan()
    {
        var source = TestSourceBuilder.Mapping("IEnumerable<int>", "Span<int>");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return (global::System.Span<int>)global::System.Linq.Enumerable.ToArray(source);");
    }

    [Fact]
    public void EnumerableToSpanOfCastedType()
    {
        var source = TestSourceBuilder.Mapping("IEnumerable<int>", "Span<long>");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                "return (global::System.Span<long>)global::System.Linq.Enumerable.ToArray(global::System.Linq.Enumerable.Select(source, x => (long)x));"
            );
    }

    [Fact]
    public void SpanToIEnumerable()
    {
        var source = TestSourceBuilder.Mapping("Span<int>", "IEnumerable<int>");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source.ToArray();");
    }

    [Fact]
    public void SpanToList()
    {
        var source = TestSourceBuilder.Mapping("Span<int>", "List<int>");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::System.Collections.Generic.List<int>(source.Length);
                foreach (var item in source)
                {
                    target.Add(item);
                }
                return target;
                """
            );
    }

    [Fact]
    public void SpanToStack()
    {
        var source = TestSourceBuilder.Mapping("Span<int>", "Stack<int>");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::System.Collections.Generic.Stack<int>(source.Length);
                foreach (var item in source)
                {
                    target.Push(item);
                }
                return target;
                """
            );
    }

    [Fact]
    public void SpanToQueue()
    {
        var source = TestSourceBuilder.Mapping("Span<int>", "Queue<int>");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::System.Collections.Generic.Queue<int>(source.Length);
                foreach (var item in source)
                {
                    target.Enqueue(item);
                }
                return target;
                """
            );
    }

    [Fact]
    public Task SpanToICollection()
    {
        var source = TestSourceBuilder.Mapping("Span<int>", "ICollection<int>");

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task SpanToIList()
    {
        var source = TestSourceBuilder.Mapping("Span<int>", "IList<int>");

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void SpanToImmutableList()
    {
        var source = TestSourceBuilder.Mapping("Span<int>", "System.Collections.Immutable.ImmutableList<int>");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return global::System.Collections.Immutable.ImmutableList.ToImmutableList(source.ToArray());");
    }

    [Fact]
    public void SpanToSpanDeepCloning()
    {
        var source = TestSourceBuilder.Mapping("Span<int>", "Span<int>", TestSourceBuilderOptions.WithDeepCloning);
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source.ToArray();");
    }

    [Fact]
    public void SpanToReadOnlySpanDeepCloning()
    {
        var source = TestSourceBuilder.Mapping("Span<int>", "ReadOnlySpan<int>", TestSourceBuilderOptions.WithDeepCloning);
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source.ToArray();");
    }

    [Fact]
    public void ReadOnlySpanToReadOnlySpanDeepCloning()
    {
        var source = TestSourceBuilder.Mapping("ReadOnlySpan<int>", "ReadOnlySpan<int>", TestSourceBuilderOptions.WithDeepCloning);
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source.ToArray();");
    }

    [Fact]
    public void ReadOnlySpanToSpanDeepCloning()
    {
        var source = TestSourceBuilder.Mapping("ReadOnlySpan<int>", "Span<int>", TestSourceBuilderOptions.WithDeepCloning);
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source.ToArray();");
    }

    [Fact]
    public void SpanToSpanWithClassTypeDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "Span<A>",
            "Span<A>",
            TestSourceBuilderOptions.WithDeepCloning,
            "class A { public int Value { get; set; } }"
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
    public void SpanToSpanOfCastedTypesDeepCloning()
    {
        var source = TestSourceBuilder.Mapping("Span<int>", "Span<long>", TestSourceBuilderOptions.WithDeepCloning);
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new long[source.Length];
                for (var i = 0; i < source.Length; i++)
                {
                    target[i] = (long)source[i];
                }
                return target;
                """
            );
    }

    [Fact]
    public void SpanToReadOnlySpanOfCastedTypesDeepCloning()
    {
        var source = TestSourceBuilder.Mapping("Span<int>", "ReadOnlySpan<long>", TestSourceBuilderOptions.WithDeepCloning);
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new long[source.Length];
                for (var i = 0; i < source.Length; i++)
                {
                    target[i] = (long)source[i];
                }
                return target;
                """
            );
    }

    [Fact]
    public void ReadOnlySpanToReadOnlySpanOfCastedTypesDeepCloning()
    {
        var source = TestSourceBuilder.Mapping("ReadOnlySpan<int>", "ReadOnlySpan<long>", TestSourceBuilderOptions.WithDeepCloning);
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new long[source.Length];
                for (var i = 0; i < source.Length; i++)
                {
                    target[i] = (long)source[i];
                }
                return target;
                """
            );
    }

    [Fact]
    public void ReadOnlySpanToSpanOfCastedTypesDeepCloning()
    {
        var source = TestSourceBuilder.Mapping("ReadOnlySpan<int>", "Span<long>", TestSourceBuilderOptions.WithDeepCloning);
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new long[source.Length];
                for (var i = 0; i < source.Length; i++)
                {
                    target[i] = (long)source[i];
                }
                return target;
                """
            );
    }

    [Fact]
    public void ArrayToReadOnlySpanDeepCloning()
    {
        var source = TestSourceBuilder.Mapping("int[]", "ReadOnlySpan<int>", TestSourceBuilderOptions.WithDeepCloning);
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source.ToArray();");
    }

    [Fact]
    public void ArrayToReadOnlySpanWithClassTypeDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "A[]",
            "ReadOnlySpan<A>",
            TestSourceBuilderOptions.WithDeepCloning,
            "class A { public int Value { get; set; } }"
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
    public void ArrayToReadOnlySpanOfCastedTypesDeepCloning()
    {
        var source = TestSourceBuilder.Mapping("int[]", "ReadOnlySpan<long>", TestSourceBuilderOptions.WithDeepCloning);
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new long[source.Length];
                for (var i = 0; i < source.Length; i++)
                {
                    target[i] = (long)source[i];
                }
                return target;
                """
            );
    }

    [Fact]
    public void ReadOnlySpanToArrayDeepCloning()
    {
        var source = TestSourceBuilder.Mapping("ReadOnlySpan<int>", "int[]", TestSourceBuilderOptions.WithDeepCloning);
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source.ToArray();");
    }

    [Fact]
    public void ReadOnlySpanToArrayWithClassTypeDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "ReadOnlySpan<A>",
            "A[]",
            TestSourceBuilderOptions.WithDeepCloning,
            "class A { public int Value { get; set; } }"
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
    public void ReadOnlySpanToArrayOfCastedTypesDeepCloning()
    {
        var source = TestSourceBuilder.Mapping("ReadOnlySpan<int>", "long[]", TestSourceBuilderOptions.WithDeepCloning);
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new long[source.Length];
                for (var i = 0; i < source.Length; i++)
                {
                    target[i] = (long)source[i];
                }
                return target;
                """
            );
    }

    [Fact]
    public void EnumerableToSpanDeepCloning()
    {
        var source = TestSourceBuilder.Mapping("IEnumerable<int>", "Span<int>", TestSourceBuilderOptions.WithDeepCloning);
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return (global::System.Span<int>)global::System.Linq.Enumerable.ToArray(source);");
    }

    [Fact]
    public void EnumerableToSpanOfCastedTypeDeepCloning()
    {
        var source = TestSourceBuilder.Mapping("IEnumerable<int>", "Span<long>", TestSourceBuilderOptions.WithDeepCloning);
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                "return (global::System.Span<long>)global::System.Linq.Enumerable.ToArray(global::System.Linq.Enumerable.Select(source, x => (long)x));"
            );
    }

    [Fact]
    public void SpanToReadonlySpanShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public Span<int> Value { get; } }",
            "class B { public Span<int> Value { get; } }"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                Riok.Mapperly.Diagnostics.DiagnosticDescriptors.CannotMapToReadOnlyType,
                "Cannot map to read-only type System.Span<int>"
            )
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void ArrayToReadonlySpanShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public int[] Value { get; } }",
            "class B { public ReadOnlySpan<int> Value { get; } }"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.CannotMapToReadOnlyType, "Cannot map to read-only type System.ReadOnlySpan<int>")
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void SpanToReadOnlyList()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public Span<int> Value { get; } }",
            "class B { public List<int> Value { get; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value.EnsureCapacity(source.Value.Length + target.Value.Count);
                foreach (var item in source.Value)
                {
                    target.Value.Add(item);
                }
                return target;
                """
            );
    }

    [Fact]
    public void EnumerableToReadonlySpanShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IEnumerable<int> Value { get; } }",
            "class B { public Span<int> Value { get; } }"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.CannotMapToReadOnlyType, "Cannot map to read-only type System.Span<int>")
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void SpanMappingDisabledShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "Span<long>",
            "Span<int>",
            TestSourceBuilderOptions.WithDisabledMappingConversion(MappingConversionType.Span)
        );
        TestHelper.GenerateMapper(source, TestHelperOptions.AllowDiagnostics).Should().HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void SpanMappingDisabledForReadOnlyShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            TestSourceBuilderOptions.WithDisabledMappingConversion(MappingConversionType.Span),
            "class A { public Span<int> Value { get; } }",
            "class B { public List<int> Value { get; } }"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotMapped);
    }

    [Fact]
    public void EnumerableMappingDisabledForReadOnlyShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            TestSourceBuilderOptions.WithDisabledMappingConversion(MappingConversionType.Enumerable),
            "class A { public Span<int> Value { get; } }",
            "class B { public List<int> Value { get; } }"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotMapped);
    }
}
