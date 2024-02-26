using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

[UsesVerify]
public class RequiredMappingTest
{
    [Fact]
    public void ClassAttributeRequiredMappingSource()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            TestSourceBuilderOptions.WithRequiredMappingStrategy(RequiredMappingStrategy.Source),
            "class A { public int Value { get; set; } public int UnmappedSource { get; set; } }",
            "class B { public int Value { get; set; } public int UnmappedTarget { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotMapped)
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = source.Value;
                return target;
                """
            );
    }

    [Fact]
    public void ClassAttributeRequiredMappingTarget()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            TestSourceBuilderOptions.WithRequiredMappingStrategy(RequiredMappingStrategy.Target),
            "class A { public int Value { get; set; } public int UnmappedSource { get; set; } }",
            "class B { public int Value { get; set; } public int UnmappedTarget { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotFound)
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = source.Value;
                return target;
                """
            );
    }

    [Fact]
    public void ClassAttributeRequiredMappingBoth()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            TestSourceBuilderOptions.WithRequiredMappingStrategy(RequiredMappingStrategy.Both),
            "class A { public int Value { get; set; } public int UnmappedSource { get; set; } }",
            "class B { public int Value { get; set; } public int UnmappedTarget { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotFound)
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotMapped)
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = source.Value;
                return target;
                """
            );
    }

    [Fact]
    public void ClassAttributeRequiredMappingNone()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            TestSourceBuilderOptions.WithRequiredMappingStrategy(RequiredMappingStrategy.None),
            "class A { public int Value { get; set; } public int UnmappedSource { get; set; } }",
            "class B { public int Value { get; set; } public int UnmappedTarget { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = source.Value;
                return target;
                """
            );
    }

    [Fact]
    public void MethodAttributeRequiredMappingSource()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapperRequiredMappingAttribute(RequiredMappingStrategy.Source)] partial B Map(A source);",
            "class A { public int Value { get; set; } public int UnmappedSource { get; set; } }",
            "class B { public int Value { get; set; } public int UnmappedTarget { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotMapped)
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = source.Value;
                return target;
                """
            );
    }

    [Fact]
    public void MethodAttributeRequiredMappingTarget()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapperRequiredMappingAttribute(RequiredMappingStrategy.Target)] partial B Map(A source);",
            "class A { public int Value { get; set; } public int UnmappedSource { get; set; } }",
            "class B { public int Value { get; set; } public int UnmappedTarget { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotFound)
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = source.Value;
                return target;
                """
            );
    }

    [Fact]
    public void MethodAttributeRequiredMappingBoth()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapperRequiredMappingAttribute(RequiredMappingStrategy.Both)] partial B Map(A source);",
            "class A { public int Value { get; set; } public int UnmappedSource { get; set; } }",
            "class B { public int Value { get; set; } public int UnmappedTarget { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotFound)
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotMapped)
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = source.Value;
                return target;
                """
            );
    }

    [Fact]
    public void MethodAttributeRequiredMappingNone()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapperRequiredMappingAttribute(RequiredMappingStrategy.None)] partial B Map(A source);",
            "class A { public int Value { get; set; } public int UnmappedSource { get; set; } }",
            "class B { public int Value { get; set; } public int UnmappedTarget { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics)
            .Should()
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = source.Value;
                return target;
                """
            );
    }

    [Fact]
    public void MethodAttributeOverridesClass()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapperRequiredMappingAttribute(RequiredMappingStrategy.Target)] partial B Map(A source);",
            TestSourceBuilderOptions.WithRequiredMappingStrategy(RequiredMappingStrategy.Source),
            "class A { public int Value { get; set; } public int UnmappedSource { get; set; } }",
            "class B { public int Value { get; set; } public int UnmappedTarget { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotFound)
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = source.Value;
                return target;
                """
            );
    }

    [Fact]
    public void AllowUnmappedTargetTupleShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "(int MyValue, int Value)",
            TestSourceBuilderOptions.WithRequiredMappingStrategy(RequiredMappingStrategy.Target),
            "class A { public int Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.NoConstructorFound)
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotFound)
            .HaveAssertedAllDiagnostics();
    }
}
