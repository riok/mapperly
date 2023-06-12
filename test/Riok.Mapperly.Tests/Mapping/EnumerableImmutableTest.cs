using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class EnumerableImmutableTest
{
    [Fact]
    public void EnumerableToImmutableList()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IEnumerable<int> Value { get; } }",
            "class B { public System.Collections.Immutable.ImmutableList<int> Value { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.Value = global::System.Collections.Immutable.ImmutableList.ToImmutableList(source.Value);
                return target;
                """
            );
    }

    [Fact]
    public void EnumerableToIImmutableList()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IEnumerable<int> Value { get; } }",
            "class B { public System.Collections.Immutable.IImmutableList<int> Value { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.Value = global::System.Collections.Immutable.ImmutableList.ToImmutableList(source.Value);
                return target;
                """
            );
    }

    [Fact]
    public void EnumerableToImmutableListOfCastedTypes()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IEnumerable<int> Value { get; } }",
            "class B { public System.Collections.Immutable.ImmutableList<long> Value { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.Value = global::System.Collections.Immutable.ImmutableList.ToImmutableList(global::System.Linq.Enumerable.Select(source.Value, x => (long)x));
                return target;
                """
            );
    }

    [Fact]
    public void EnumerableToImmutableArray()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IEnumerable<int> Value { get; } }",
            "class B { public System.Collections.Immutable.ImmutableArray<int> Value { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.Value = global::System.Collections.Immutable.ImmutableArray.ToImmutableArray(source.Value);
                return target;
                """
            );
    }

    [Fact]
    public void EnumerableToImmutableHashSet()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IEnumerable<int> Value { get; } }",
            "class B { public System.Collections.Immutable.ImmutableHashSet<int> Value { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.Value = global::System.Collections.Immutable.ImmutableHashSet.ToImmutableHashSet(source.Value);
                return target;
                """
            );
    }

    [Fact]
    public void EnumerableToIImmutableSet()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IEnumerable<int> Value { get; } }",
            "class B { public System.Collections.Immutable.IImmutableSet<int> Value { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.Value = global::System.Collections.Immutable.ImmutableHashSet.ToImmutableHashSet(source.Value);
                return target;
                """
            );
    }

    [Fact]
    public void EnumerableToImmutableQueue()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IEnumerable<int> Value { get; } }",
            "class B { public System.Collections.Immutable.ImmutableQueue<int> Value { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.Value = global::System.Collections.Immutable.ImmutableQueue.CreateRange(source.Value);
                return target;
                """
            );
    }

    [Fact]
    public void EnumerableToIImmutableQueue()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IEnumerable<int> Value { get; } }",
            "class B { public System.Collections.Immutable.IImmutableQueue<int> Value { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.Value = global::System.Collections.Immutable.ImmutableQueue.CreateRange(source.Value);
                return target;
                """
            );
    }

    [Fact]
    public void EnumerableToImmutableStack()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IEnumerable<int> Value { get; } }",
            "class B { public System.Collections.Immutable.ImmutableStack<int> Value { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.Value = global::System.Collections.Immutable.ImmutableStack.CreateRange(source.Value);
                return target;
                """
            );
    }

    [Fact]
    public void EnumerableToIImmutableStack()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IEnumerable<int> Value { get; } }",
            "class B { public System.Collections.Immutable.IImmutableStack<int> Value { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.Value = global::System.Collections.Immutable.ImmutableStack.CreateRange(source.Value);
                return target;
                """
            );
    }

    [Fact]
    public void EnumerableToImmutableSortedSet()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IEnumerable<int> Value { get; } }",
            "class B { public System.Collections.Immutable.ImmutableSortedSet<int> Value { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.Value = global::System.Collections.Immutable.ImmutableSortedSet.ToImmutableSortedSet(source.Value);
                return target;
                """
            );
    }

    [Fact]
    public void EnumerableToReadOnlyImmutableListShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IEnumerable<int> Value { get; } }",
            "class B { public System.Collections.Immutable.ImmutableList<int> Value { get; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.CannotMapToReadOnlyMember)
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void EnumerableToReadOnlyImmutableArrayShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IEnumerable<int> Value { get; } }",
            "class B { public System.Collections.Immutable.ImmutableArray<int> Value { get; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.CannotMapToReadOnlyMember);
    }

    [Fact]
    public void EnumerableToReadOnlyImmutableHashSetShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IEnumerable<int> Value { get; } }",
            "class B { public System.Collections.Immutable.ImmutableHashSet<int> Value { get; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.CannotMapToReadOnlyMember)
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void EnumerableToReadOnlyImmutableQueueShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IEnumerable<int> Value { get; } }",
            "class B { public System.Collections.Immutable.ImmutableQueue<int> Value { get; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.CannotMapToReadOnlyMember)
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void EnumerableToReadOnlyImmutableStackShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IEnumerable<int> Value { get; } }",
            "class B { public System.Collections.Immutable.ImmutableStack<int> Value { get; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.CannotMapToReadOnlyMember)
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void EnumerableToReadOnlyImmutableSortedSetShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IEnumerable<int> Value { get; } }",
            "class B { public System.Collections.Immutable.ImmutableSortedSet<int> Value { get; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.CannotMapToReadOnlyMember)
            .HaveAssertedAllDiagnostics();
    }
}
