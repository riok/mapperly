using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class RequireExplicitMappingTest
{
    [Fact]
    public void RequireExplicitMappingTrueWithoutExplicitMappingReportsDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "private partial ATarget MapToA(A source);",
            TestSourceBuilderOptions.WithRequireExplicitMapping,
            "class A { public B B { get; set; } }",
            "class ATarget { public BTarget B { get; set; } }",
            "class B { }",
            "class BTarget { }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.ExplicitMappingRequired)
            .HaveDiagnostic(DiagnosticDescriptors.CouldNotMapMember)
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotMapped)
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotFound)
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void RequireExplicitMappingTrueWithExplicitMappingSucceeds()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private partial ATarget MapToA(A source);
            private partial BTarget MapToB(B source);
            """,
            TestSourceBuilderOptions.WithRequireExplicitMapping,
            "class A { public B B { get; set; } }",
            "class ATarget { public BTarget B { get; set; } }",
            "class B { }",
            "class BTarget { }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMethodBody(
                "MapToA",
                """
                var target = new global::ATarget();
                target.B = MapToB(source.B);
                return target;
                """
            );
    }

    [Fact]
    public void RequireExplicitMappingFalseAutoGeneratesNestedMapping()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "private partial ATarget MapToA(A source);",
            "class A { public B B { get; set; } }",
            "class ATarget { public BTarget B { get; set; } }",
            "class B { }",
            "class BTarget { }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMethodBody(
                "MapToA",
                """
                var target = new global::ATarget();
                target.B = MapToBTarget(source.B);
                return target;
                """
            );
    }

    [Fact]
    public void RequireExplicitMappingTrueWithPrimitiveTypesSucceeds()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "private partial ATarget MapToA(A source);",
            TestSourceBuilderOptions.WithRequireExplicitMapping,
            "class A { public int Value { get; set; } public string Name { get; set; } }",
            "class ATarget { public int Value { get; set; } public string Name { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::ATarget();
                target.Value = source.Value;
                target.Name = source.Name;
                return target;
                """
            );
    }

    [Fact]
    public void RequireExplicitMappingTrueWithMultipleNestedTypesReportsDiagnostics()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "private partial ATarget MapToA(A source);",
            TestSourceBuilderOptions.WithRequireExplicitMapping,
            "class A { public B B { get; set; } public C C { get; set; } }",
            "class ATarget { public BTarget B { get; set; } public CTarget C { get; set; } }",
            "class B { }",
            "class BTarget { }",
            "class C { }",
            "class CTarget { }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.ExplicitMappingRequired)
            .HaveDiagnostic(DiagnosticDescriptors.CouldNotMapMember)
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotMapped)
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotFound)
            .HaveAssertedAllDiagnostics();
    }
}
