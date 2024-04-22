using Microsoft.CodeAnalysis;
using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class ObjectNestedPropertyTest
{
    private static readonly TestHelperOptions ignoreNestedMemberNotUsed =
        new() { IgnoredDiagnostics = new HashSet<DiagnosticDescriptor> { DiagnosticDescriptors.NestedMemberNotUsed } };

    [Fact]
    public void NestedPropertyWithMemberNameOfSource()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapNestedProperties(nameof(A.Value))] partial B Map(A source);",
            "class A { public C Value { get; set; } }",
            "class B { public string Id { get; set; } }",
            "class C { public string Id { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Id = source.Value.Id;
                return target;
                """
            );
    }

    [Fact]
    public void DeeplyNestedPropertyWithMemberNameOfSource()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapNestedProperties(nameof(@A.Value.NestedValue))] partial B Map(A source);",
            "class A { public C Value { get; set; } }",
            "class B { public string Id { get; set; } }",
            "class C { public D NestedValue { get; set; } }",
            "class D { public string Id { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Id = source.Value.NestedValue.Id;
                return target;
                """
            );
    }

    [Fact]
    public void DeeplyNestedPropertyAsArrayWithMemberNameOfSource()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapNestedProperties(new [] {nameof(A.Value), nameof(C.NestedValue)})] partial B Map(A source);",
            "class A { public C Value { get; set; } }",
            "class B { public string Id { get; set; } }",
            "class C { public D NestedValue { get; set; } }",
            "class D { public string Id { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Id = source.Value.NestedValue.Id;
                return target;
                """
            );
    }

    [Fact]
    public void RootPropertiesShouldBePreferredOverNestedProperties()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapNestedProperties(nameof(A.Value))] partial B Map(A source);",
            "class A { public string Id { get; set; } public C Value { get; set; } }",
            "class B { public string Id { get; set; } }",
            "class C { public string Id { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, ignoreNestedMemberNotUsed)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Id = source.Id;
                return target;
                """
            );
    }

    [Fact]
    public void NestedPropertyWithSourcePathName()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapNestedProperties(nameof(A.Value))] partial B Map(A source);",
            "class A { public C Value { get; set; } }",
            "class B { public string ValueId { get; set; } }",
            "class C { public string ValueId { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.ValueId = source.Value.ValueId;
                return target;
                """
            );
    }

    [Fact]
    public void NestedPropertyWithSourcePathNamePrefersAutoFlattenedCompletePath()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapNestedProperties(nameof(A.Value))] partial B Map(A source);",
            "class A { public C Value { get; set; } }",
            "class B { public string ValueId { get; set; } }",
            "class C { public string Id { get; set; } public string ValueId { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, ignoreNestedMemberNotUsed)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.ValueId = source.Value.Id;
                return target;
                """
            );
    }

    [Fact]
    public void UnusedNestedPropertiesShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapNestedProperties(nameof(A.Value))] partial B Map(A source);",
            "class A { public string Id { get; set; } public C Value { get; set; } }",
            "class B { public string Id { get; set; } }",
            "class C { public string MyId { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.NestedMemberNotUsed)
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotMapped)
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Id = source.Id;
                return target;
                """
            );
    }

    [Fact]
    public void UnusedNestedPropertiesShouldDiagnosticEvenIfPropertyIsUsed()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapNestedProperties(nameof(A.Value))] partial B Map(A source);",
            "class A { public C Value { get; set; } }",
            "class B { public C Value { get; set; } }",
            "class C { public string MyId { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.NestedMemberNotUsed)
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
    public void InvalidNestedPropertiesPathShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapNestedProperties(\"Value\")] partial B Map(A source);",
            "class A { public string Id { get; set; } }",
            "class B { public string Id { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.ConfiguredMappingNestedMemberNotFound)
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void IgnoredNestedPropertyShouldNotDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapperIgnoreSource(nameof(A.Value))]
            [MapNestedProperties(nameof(A.Value))]
            partial B Map(A source);
            """,
            "class A { public C Value { get; set; } }",
            "class B { public string Id { get; set; } }",
            "class C { public string Id { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Id = source.Value.Id;
                return target;
                """
            );
    }

    [Fact]
    public void IgnoredNestedPropertyShouldBePreferredOverAutoFlattened()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapperIgnoreSource(nameof(A.Value))]
            [MapNestedProperties(nameof(A.Value))]
            partial B Map(A source);
            """,
            "class A { public C Value { get; set; } }",
            "class B { public string ValueId { get; set; } }",
            "class C { public string Id { get; set; } public string ValueId { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.ValueId = source.Value.ValueId;
                return target;
                """
            );
    }
}
