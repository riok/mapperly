using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class ObjectNestedPropertyTest
{
    private static readonly TestHelperOptions _ignoreNestedMemberNotUsed =
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
            .GenerateMapper(source, _ignoreNestedMemberNotUsed)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Id = source.Id;
                return target;
                """
            );
    }

    /// <summary>
    /// Same as <see cref="NestedPropertyWithMemberNameOfSource"/>, but the property name being <c>ValueId</c> means that auto-flattening will be tried first.
    /// This checks that the nested member lookup works correctly afterward as well.
    /// </summary>
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
            .GenerateMapper(source, _ignoreNestedMemberNotUsed)
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
    public void NestedPropertyToConstructorParameter()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapNestedProperties(nameof(A.Value))] partial B Map(A source);",
            "class A { public C Value { get; set; } }",
            "class B { public B(int nestedId) {} }",
            "class C { public int NestedId { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B(source.Value.NestedId);
                return target;
                """
            );
    }

    [Fact]
    public void NestedPropertyToInitOnlyProperty()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapNestedProperties(nameof(A.Value))] partial B Map(A source);",
            "class A { public C Value { get; set; } }",
            "class B { public int NestedId { get; init; } }",
            "class C { public int NestedId { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B()
                {
                    NestedId = source.Value.NestedId,
                };
                return target;
                """
            );
    }

    [Fact]
    public void NestedPropertyToTuple()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapNestedProperties(nameof(A.Value))] partial (int NestedId, string ValueName) Map(A source);",
            "class A { public C Value { get; set; } }",
            "class C { public int NestedId { get; set; } public string Name { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = (NestedId: source.Value.NestedId, ValueName: source.Value.Name);
                return target;
                """
            );
    }

    [Fact]
    public void NestedPropertyWithMemberNameOfSourceAndCaseInsensitiveNameMappingStrategy()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapNestedProperties(nameof(A.Value))] partial B Map(A source);",
            new TestSourceBuilderOptions { PropertyNameMappingStrategy = PropertyNameMappingStrategy.CaseInsensitive },
            "class A { public C Value { get; set; } }",
            "class B { public string id { get; set; } }",
            "class C { public string Id { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.id = source.Value.Id;
                return target;
                """
            );
    }

    [Fact]
    public void NullableNestedPropertyWithMemberNameOfSource()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapNestedProperties(nameof(A.Value))] partial B Map(A source);",
            "class A { public C? Value { get; set; } }",
            "class B { public string Id { get; set; } }",
            "class C { public string Id { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.NullableSourceValueToNonNullableTargetValue,
                "Mapping the nullable source property Value.Id of A to the target property Id of B which is not nullable"
            )
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                if (source.Value != null)
                {
                    target.Id = source.Value.Id;
                }
                return target;
                """
            );
    }

    [Fact]
    public Task UnusedNestedPropertiesShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapNestedProperties(nameof(A.Value))] partial B Map(A source);",
            "class A { public string Id { get; set; } public C Value { get; set; } }",
            "class B { public string Id { get; set; } }",
            "class C { public string MyId { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
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
    public Task InvalidNestedPropertiesPathShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapNestedProperties(\"Value\")] partial B Map(A source);",
            "class A { public string Id { get; set; } }",
            "class B { public string Id { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
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
