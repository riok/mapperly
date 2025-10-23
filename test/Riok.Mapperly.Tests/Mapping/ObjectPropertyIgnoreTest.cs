using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class ObjectPropertyIgnoreTest
{
    [Fact]
    public void WithIgnoredSourceAndTargetPropertyShouldIgnore()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapperIgnoreSource(nameof(A.IntValue))] [MapperIgnoreTarget(nameof(B.IntValue))] partial B Map(A source);",
            "class A { public string StringValue { get; set; } public int IntValue { get; set; } }",
            "class B { public string StringValue { get; set; }  public int IntValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.StringValue = source.StringValue;
                return target;
                """
            );
    }

    [Fact]
    public void ExistingTargetWithIgnoredSourceAndTargetPropertyShouldIgnore()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapperIgnoreSource(nameof(A.StringValue)), MapperIgnoreTarget(nameof(B.StringValue2))] partial void Map(A source, B target);",
            "class A { public string StringValue { get; set; } public string StringValue2 { get; set; } public int IntValue { get; set; } }",
            "class B { public string StringValue { get; set; } public string StringValue2 { get; set; } public int IntValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.SourceMemberNotMapped,
                "The member StringValue2 on the mapping source type A is not mapped to any member on the mapping target type B"
            )
            .HaveDiagnostic(
                DiagnosticDescriptors.SourceMemberNotFound,
                "The member StringValue on the mapping target type B was not found on the mapping source type A"
            )
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody("target.IntValue = source.IntValue;");
    }

    [Fact]
    public void WithIgnoredSourcePropertyShouldIgnoreAndGenerateDiagnostics()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapperIgnoreSource(nameof(A.IntValue))] partial B Map(A source);",
            "class A { public string StringValue { get; set; } public int IntValue { get; set; } }",
            "class B { public string StringValue { get; set; }  public int IntValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.SourceMemberNotFound,
                "The member IntValue on the mapping target type B was not found on the mapping source type A"
            )
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.StringValue = source.StringValue;
                return target;
                """
            );
    }

    [Fact]
    public void WithIgnoredTargetPropertyShouldIgnoreAndGenerateDiagnostics()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapperIgnoreTarget(nameof(B.IntValue))] partial B Map(A source);",
            "class A { public string StringValue { get; set; } public int IntValue { get; set; } }",
            "class B { public string StringValue { get; set; }  public int IntValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.SourceMemberNotMapped,
                "The member IntValue on the mapping source type A is not mapped to any member on the mapping target type B"
            )
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.StringValue = source.StringValue;
                return target;
                """
            );
    }

    [Fact]
    public void WithNotFoundIgnoredTargetPropertyShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapperIgnoreTarget(\"not_found\")] partial B Map(A source);",
            "class A { }",
            "class B { }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.IgnoredTargetMemberNotFound, "Ignored target member not_found on B was not found")
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                return target;
                """
            );
    }

    [Fact]
    public void WithNotFoundIgnoredSourcePropertyShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapperIgnoreSource(\"not_found\")] partial B Map(A source);",
            "class A { }",
            "class B { }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.IgnoredSourceMemberNotFound, "Ignored source member not_found on A was not found")
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                return target;
                """
            );
    }

    [Fact]
    public void WithNestedIgnoredSourceAndTargetPropertyShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapperIgnoreSource(\"StringValue.Value\")] [MapperIgnoreTarget(\"StringValue.Value\")] partial B Map(A source);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.NestedIgnoredSourceMember)
            .HaveDiagnostic(DiagnosticDescriptors.NestedIgnoredTargetMember)
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.StringValue = source.StringValue;
                return target;
                """
            );
    }

    [Fact]
    public void OnlyExplicitMappedMembersWithExtraSourceMembers()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A source);",
            new TestSourceBuilderOptions(OnlyExplicitMappedMembers: true),
            "class A { public int Value1 { get; set; } public int Value2 { get; set; } public int Value4 { get; set; } }",
            "class B { public int Value1 { get; set; } public int Value2 { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value1 = source.Value1;
                target.Value2 = source.Value2;
                return target;
                """
            );
    }

    [Fact]
    public void OnlyExplicitMappedMembersWithExtraTargetMembers()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A source);",
            new TestSourceBuilderOptions(OnlyExplicitMappedMembers: true),
            "class A { public int Value1 { get; set; } public int Value2 { get; set; } }",
            "class B { public int Value1 { get; set; } public int Value2 { get; set; } public int Value4 { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value1 = source.Value1;
                target.Value2 = source.Value2;
                return target;
                """
            );
    }

    [Fact]
    public void OnlyExplicitMappedMembersWithExtraMembersInBoth()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A source);",
            new TestSourceBuilderOptions(OnlyExplicitMappedMembers: true),
            "class A { public int Value1 { get; set; } public int Value2 { get; set; } public int Value4 { get; set; } }",
            "class B { public int Value1 { get; set; } public int Value2 { get; set; } public int Value3 { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value1 = source.Value1;
                target.Value2 = source.Value2;
                return target;
                """
            );
    }
}
