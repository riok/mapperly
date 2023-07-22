using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

[UsesVerify]
public class MemoryTest
{
    [Fact]
    public void MemoryToMemory()
    {
        var source = TestSourceBuilder.Mapping("Memory<int>", "Memory<int>");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source;");
    }

    [Fact]
    public void MemoryToReadOnlyMemory()
    {
        var source = TestSourceBuilder.Mapping("Memory<int>", "ReadOnlyMemory<int>");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return (global::System.ReadOnlyMemory<int>)source;");
    }

    [Fact]
    public void ReadOnlyMemoryToReadOnlyMemory()
    {
        var source = TestSourceBuilder.Mapping("ReadOnlyMemory<int>", "ReadOnlyMemory<int>");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source;");
    }

    [Fact]
    public void ReadOnlyMemoryToMemory()
    {
        var source = TestSourceBuilder.Mapping("ReadOnlyMemory<int>", "Memory<int>");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source.ToArray();");
    }

    [Fact]
    public void MemoryToMemoryOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping("Memory<int>", "Memory<long>");
        TestHelper.GenerateMapper(source).Should().HaveMapMethodBody("return MapToInt64Array(source.Span);");
    }

    [Fact]
    public void MemoryToReadOnlyMemoryOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping("Memory<int>", "ReadOnlyMemory<long>");
        TestHelper.GenerateMapper(source).Should().HaveMapMethodBody("return MapToInt64Array(source.Span);");
    }

    [Fact]
    public void ReadOnlyMemoryToReadOnlyMemoryOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping("ReadOnlyMemory<int>", "ReadOnlyMemory<long>");
        TestHelper.GenerateMapper(source).Should().HaveMapMethodBody("return MapToInt64Array(source.Span);");
    }

    [Fact]
    public void ReadOnlyMemoryToMemoryOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping("ReadOnlyMemory<int>", "Memory<long>");
        TestHelper.GenerateMapper(source).Should().HaveMapMethodBody("return MapToInt64Array(source.Span);");
    }

    [Fact]
    public void MemoryToMemoryUseDeepCloning()
    {
        var source = TestSourceBuilder.Mapping("Memory<int>", "Memory<int>", TestSourceBuilderOptions.WithDeepCloning);
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source.Span.ToArray();");
    }

    [Fact]
    public void ReadOnlyMemoryToMemoryUseDeepCloning()
    {
        var source = TestSourceBuilder.Mapping("ReadOnlyMemory<int>", "Memory<int>", TestSourceBuilderOptions.WithDeepCloning);
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source.Span.ToArray();");
    }

    [Fact]
    public void MemoryToSpan()
    {
        var source = TestSourceBuilder.Mapping("Memory<int>", "Span<int>");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source.Span;");
    }

    [Fact]
    public void MemoryToReadOnlySpan()
    {
        var source = TestSourceBuilder.Mapping("Memory<int>", "ReadOnlySpan<int>");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source.Span;");
    }

    [Fact]
    public void ReadOnlyMemoryToReadOnlySpan()
    {
        var source = TestSourceBuilder.Mapping("ReadOnlyMemory<int>", "ReadOnlySpan<int>");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source.Span;");
    }

    [Fact]
    public void ReadOnlyMemoryToSpan()
    {
        var source = TestSourceBuilder.Mapping("ReadOnlyMemory<int>", "Span<int>");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source.ToArray();");
    }

    [Fact]
    public void MemoryToSpanOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping("Memory<int>", "Span<long>");
        TestHelper.GenerateMapper(source).Should().HaveMapMethodBody("return MapToSpan(source.Span);");
    }

    [Fact]
    public void MemoryToReadOnlySpanOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping("Memory<int>", "ReadOnlySpan<long>");
        TestHelper.GenerateMapper(source).Should().HaveMapMethodBody("return MapToReadOnlySpan(source.Span);");
    }

    [Fact]
    public void ReadOnlyMemoryToReadOnlySpanOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping("ReadOnlyMemory<int>", "ReadOnlySpan<long>");
        TestHelper.GenerateMapper(source).Should().HaveMapMethodBody("return MapToReadOnlySpan(source.Span);");
    }

    [Fact]
    public void ReadOnlyMemoryToSpanOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping("ReadOnlyMemory<int>", "Span<long>");
        TestHelper.GenerateMapper(source).Should().HaveMapMethodBody("return MapToInt64Array(source.Span);");
    }

    [Fact]
    public void MemoryToSpanUseDeepCloning()
    {
        var source = TestSourceBuilder.Mapping("Memory<int>", "Span<int>", TestSourceBuilderOptions.WithDeepCloning);
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source.Span.ToArray();");
    }

    [Fact]
    public void ReadOnlyMemoryToSpanCloning()
    {
        var source = TestSourceBuilder.Mapping("ReadOnlyMemory<int>", "Span<int>", TestSourceBuilderOptions.WithDeepCloning);
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source.Span.ToArray();");
    }

    [Fact]
    public void SpanToMemory()
    {
        var source = TestSourceBuilder.Mapping("Span<int>", "Memory<int>");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source.ToArray();");
    }

    [Fact]
    public void SpanToReadOnlyMemory()
    {
        var source = TestSourceBuilder.Mapping("Span<int>", "ReadOnlyMemory<int>");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source.ToArray();");
    }

    [Fact]
    public void ReadOnlySpanToReadOnlyMemory()
    {
        var source = TestSourceBuilder.Mapping("ReadOnlySpan<int>", "ReadOnlyMemory<int>");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source.ToArray();");
    }

    [Fact]
    public void ReadOnlySpanToMemory()
    {
        var source = TestSourceBuilder.Mapping("ReadOnlySpan<int>", "Memory<int>");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source.ToArray();");
    }

    [Fact]
    public void SpanToMemoryOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping("Span<int>", "Memory<long>");
        TestHelper.GenerateMapper(source).Should().HaveMapMethodBody("return (global::System.Memory<long>)MapToInt64Array(source);");
    }

    [Fact]
    public void SpanToReadOnlyMemoryOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping("Span<int>", "ReadOnlyMemory<long>");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody("return (global::System.ReadOnlyMemory<long>)MapToInt64Array(source);");
    }

    [Fact]
    public void ReadOnlySpanToReadOnlyMemoryOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping("ReadOnlySpan<int>", "ReadOnlyMemory<long>");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody("return (global::System.ReadOnlyMemory<long>)MapToInt64Array(source);");
    }

    [Fact]
    public void ReadOnlySpanToMemoryOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping("ReadOnlySpan<int>", "Memory<long>");
        TestHelper.GenerateMapper(source).Should().HaveMapMethodBody("return (global::System.Memory<long>)MapToInt64Array(source);");
    }

    [Fact]
    public void SpanToMemoryUseDeepCloning()
    {
        var source = TestSourceBuilder.Mapping("Span<int>", "Memory<int>", TestSourceBuilderOptions.WithDeepCloning);
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return (global::System.Memory<int>)source.ToArray();");
    }

    [Fact]
    public void MemoryToArray()
    {
        var source = TestSourceBuilder.Mapping("Memory<int>", "int[]");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source.ToArray();");
    }

    [Fact]
    public void ReadOnlyMemoryToArray()
    {
        var source = TestSourceBuilder.Mapping("ReadOnlyMemory<int>", "int[]");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source.ToArray();");
    }

    [Fact]
    public void MemoryToArrayOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping("Memory<int>", "long[]");
        TestHelper.GenerateMapper(source).Should().HaveMapMethodBody("return MapToInt64Array(source.Span);");
    }

    [Fact]
    public void ReadOnlyMemoryToArrayOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping("ReadOnlyMemory<int>", "long[]");
        TestHelper.GenerateMapper(source).Should().HaveMapMethodBody("return MapToInt64Array(source.Span);");
    }

    [Fact]
    public void ReadOnlyMemoryToIEnumerable()
    {
        var source = TestSourceBuilder.Mapping("ReadOnlyMemory<int>", "IEnumerable<int>");
        TestHelper.GenerateMapper(source).Should().HaveMapMethodBody("return source.Span.ToArray();");
    }

    [Fact]
    public Task ReadOnlyMemoryToList()
    {
        var source = TestSourceBuilder.Mapping("ReadOnlyMemory<int>", "List<int>");

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ReadOnlyMemoryToStack()
    {
        var source = TestSourceBuilder.Mapping("ReadOnlyMemory<int>", "Stack<int>");

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void ReadOnlyMemoryToImmutableList()
    {
        var source = TestSourceBuilder.Mapping("ReadOnlyMemory<int>", "System.Collections.Immutable.ImmutableList<int>");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody("return global::System.Collections.Immutable.ImmutableList.ToImmutableList(source.Span.ToArray());");
    }

    [Fact]
    public void ReadOnlyMemoryToEnumerableOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping("ReadOnlyMemory<int>", "IEnumerable<int>");
        TestHelper.GenerateMapper(source).Should().HaveMapMethodBody("return source.Span.ToArray();");
    }

    [Fact]
    public void ArrayToReadOnlyMemory()
    {
        var source = TestSourceBuilder.Mapping("int[]", "ReadOnlyMemory<int>");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return (global::System.ReadOnlyMemory<int>)source;");
    }

    [Fact]
    public void ArrayToReadOnlyMemoryOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping("int[]", "ReadOnlyMemory<long>");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
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
    public void IEnumerableToReadOnlyMemory()
    {
        var source = TestSourceBuilder.Mapping("IEnumerable<int>", "ReadOnlyMemory<int>");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::System.Linq.Enumerable.ToArray(source);");
    }

    [Fact]
    public void IEnumerableToReadOnlyMemoryOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping("IEnumerable<int>", "ReadOnlyMemory<long>");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                "return global::System.Linq.Enumerable.ToArray(global::System.Linq.Enumerable.Select(source, x => (long)x));"
            );
    }

    [Fact]
    public void ReadOnlyMemoryToCreatedList()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public ReadOnlyMemory<int> Value { get; } }",
            "class B { public List<int> Value { get; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
            var target = new global::B();
            target.Value.EnsureCapacity(source.Value.Span.Length + target.Value.Count);
            foreach (var item in source.Value.Span)
            {
                target.Value.Add(item);
            }

            return target;
            """
            );
    }

    [Fact]
    public void MemoryToCreatedReadOnlyMemoryShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public Memory<int> Value { get; } }",
            "class B { public ReadOnlyMemory<int> Value { get; } }"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.CannotMapToReadOnlyMember);
    }

    [Fact]
    public void MemoryToCreatedMemoryShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public Memory<int> Value { get; } }",
            "class B { public Memory<int> Value { get; } }"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.CannotMapToReadOnlyMember);
    }

    [Fact]
    public void SpanToCreatedMemoryShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public Span<int> Value { get; } }",
            "class B { public Memory<int> Value { get; } }"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.CannotMapToReadOnlyMember);
    }

    [Fact]
    public void ArrayToCreatedMemoryShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public int[] Value { get; } }",
            "class B { public Memory<int> Value { get; } }"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.CannotMapToReadOnlyMember);
    }

    [Fact]
    public void EnumerableToCreatedMemoryShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IEnumerable<int> Value { get; } }",
            "class B { public Memory<int> Value { get; } }"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.CannotMapToReadOnlyMember);
    }

    [Fact]
    public void MemoryMappingDisabledShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "Memory<long>",
            "Memory<int>",
            TestSourceBuilderOptions.WithDisabledMappingConversion(MappingConversionType.Memory)
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotMapped);
    }

    [Fact]
    public void SpanMappingDisabledForReadOnlyShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            TestSourceBuilderOptions.WithDisabledMappingConversion(MappingConversionType.Memory),
            "class A { public Memory<int> Value { get; } }",
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
            TestSourceBuilderOptions.WithDisabledMappingConversion(MappingConversionType.Memory),
            "class A { public Memory<int> Value { get; } }",
            "class B { public List<int> Value { get; } }"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotMapped);
    }
}
