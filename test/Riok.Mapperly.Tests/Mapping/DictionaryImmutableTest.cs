using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class DictionaryImmutableTest
{
    [Fact]
    public void DictionaryToImmutableDictionary()
    {
        var source = TestSourceBuilder.Mapping(
            "Dictionary<string, string>",
            "System.Collections.Immutable.ImmutableDictionary<string, string>"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                return global::System.Collections.Immutable.ImmutableDictionary.ToImmutableDictionary(source);
                """
            );
    }

    [Fact]
    public void DictionaryToIImmutableDictionary()
    {
        var source = TestSourceBuilder.Mapping(
            "Dictionary<string, string>",
            "System.Collections.Immutable.IImmutableDictionary<string, string>"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                return global::System.Collections.Immutable.ImmutableDictionary.ToImmutableDictionary(source);
                """
            );
    }

    [Fact]
    public void DictionaryToImmutableSortedDictionary()
    {
        var source = TestSourceBuilder.Mapping(
            "Dictionary<string, string>",
            "System.Collections.Immutable.ImmutableSortedDictionary<string, string>"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                return global::System.Collections.Immutable.ImmutableSortedDictionary.ToImmutableSortedDictionary(source);
                """
            );
    }

    [Fact]
    public void DictionaryToImmutableDictionaryExplicitCastedKeyValue()
    {
        var source = TestSourceBuilder.Mapping("Dictionary<long, long>", "System.Collections.Immutable.ImmutableDictionary<int, int>");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                return global::System.Collections.Immutable.ImmutableDictionary.ToImmutableDictionary(source, x => (int)x.Key, x => (int)x.Value);
                """
            );
    }

    [Fact]
    public void DictionaryToImmutableDictionaryExplicitCastedValue()
    {
        var source = TestSourceBuilder.Mapping("Dictionary<string, long>", "System.Collections.Immutable.ImmutableDictionary<string, int>");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                return global::System.Collections.Immutable.ImmutableDictionary.ToImmutableDictionary(source, x => x.Key, x => (int)x.Value);
                """
            );
    }

    [Fact]
    public void EnumerableToReadOnlyImmutableDictionaryShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public Dictionary<string, string> Value { get; } }",
            "class B { public System.Collections.Immutable.ImmutableDictionary<string, string> Value { get; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.CannotMapToReadOnlyMember)
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotMapped)
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void EnumerableToReadOnlyImmutableSortedDictionaryShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public Dictionary<string, string> Value { get; } }",
            "class B { public System.Collections.Immutable.ImmutableSortedDictionary<string, string> Value { get; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.CannotMapToReadOnlyMember)
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotMapped)
            .HaveAssertedAllDiagnostics();
    }
}
