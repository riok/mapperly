using System.Collections.Immutable;
using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.DocumentationDiagnostics;

public class MapperIgnoreJustificationTests
{
    private TestHelperOptions TestHelperOptions =>
        TestHelperOptions.Default with
        {
            IgnoredDiagnostics = TestHelperOptions
                .DefaultIgnoredDiagnostics.Except([DiagnosticDescriptors.MapperIgnoreAttributeMissingJustification])
                .ToImmutableHashSet(),
        };

    [Fact]
    public void MapperIgnoreSourceShouldNotReportAttributeWithJustification()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapperIgnoreSource(nameof(A.Value), Justification = \"Justification\")] partial B Map(A source);",
            """
            class A 
            {
                public int Value { get; set; }
            }
            """,
            """
            class B { }
            """
        );

        TestHelper.GenerateMapper(source, TestHelperOptions).Should().HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void MapperIgnoreSourceShouldReportMissingJustification()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapperIgnoreSource(nameof(A.Value))] partial B Map(A source);",
            """
            class A 
            {
                public int Value { get; set; }
            }
            """,
            """
            class B { }
            """
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.MapperIgnoreAttributeMissingJustification,
                "The ignored mapping of Value for A does not specify a Justification, consider adding one for documentation purposes."
            )
            .HaveAssertedAllDiagnostics();
    }
}
