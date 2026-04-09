using Microsoft.CodeAnalysis;
using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Diagnostics;

public class MapperIgnoreJustificationTests
{
    private static readonly TestHelperOptions _allowIgnoreJustificationDiagnostics = TestHelperOptions.AllowDiagnostics with
    {
        IgnoredDiagnostics = new HashSet<DiagnosticDescriptor> { DiagnosticDescriptors.NoMemberMappings },
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

        TestHelper.GenerateMapper(source, _allowIgnoreJustificationDiagnostics).Should().HaveAssertedAllDiagnostics();
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
            .GenerateMapper(source, _allowIgnoreJustificationDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.IgnoreMissingJustification,
                "The ignored mapping of Value does not specify a justification, consider adding one for documentation purposes"
            )
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void MapperIgnoreTargetShouldReportMissingJustification()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapperIgnoreTarget(nameof(B.Value))] partial B Map(A source);",
            "class A { }",
            "class B { public int Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, _allowIgnoreJustificationDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.IgnoreMissingJustification,
                "The ignored mapping of Value does not specify a justification, consider adding one for documentation purposes"
            )
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void MapperIgnoreSourceValueShouldReportWhitespaceJustification()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapperIgnoreSourceValue(E1.Value2, Justification = \"   \")] partial E2 Map(E1 source);",
            "enum E1 { Value1, Value2 }",
            "enum E2 { Value1 }"
        );

        TestHelper
            .GenerateMapper(source, _allowIgnoreJustificationDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.IgnoreMissingJustification,
                "The ignored mapping of Value2 does not specify a justification, consider adding one for documentation purposes"
            )
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void MapperIgnoreTargetValueShouldNotReportAttributeWithJustification()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapperIgnoreTargetValue(E2.Value2, Justification = \"The legacy enum value is intentionally skipped\")] partial E2 Map(E1 source);",
            "enum E1 { Value1 }",
            "enum E2 { Value1, Value2 }"
        );

        TestHelper.GenerateMapper(source, _allowIgnoreJustificationDiagnostics).Should().HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void MapperIgnoreMemberShouldReportMissingJustification()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A source);",
            """
            class A
            {
                [MapperIgnore]
                public int Value { get; set; }
            }
            """,
            "class B { }"
        );

        TestHelper
            .GenerateMapper(source, _allowIgnoreJustificationDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.IgnoreMissingJustification,
                "The ignored mapping of Value does not specify a justification, consider adding one for documentation purposes"
            )
            .HaveAssertedAllDiagnostics();
    }
}
