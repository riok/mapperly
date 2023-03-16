using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

[UsesVerify]
public class ObjectPropertyIgnoreTest
{
    [Fact]
    public void WithIgnoredSourceAndTargetPropertyShouldIgnore()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapperIgnoreSource(nameof(A.IntValue))] [MapperIgnoreTarget(nameof(B.IntValue))] partial B Map(A source);",
            "class A { public string StringValue { get; set; } public int IntValue { get; set; } }",
            "class B { public string StringValue { get; set; }  public int IntValue { get; set; } }");

        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new B();
                target.StringValue = source.StringValue;
                return target;
                """);
    }

    [Fact]
    public void ExistingTargetWithIgnoredSourceAndTargetPropertyShouldIgnore()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapperIgnoreSource(nameof(A.StringValue)), MapperIgnoreTarget(nameof(B.StringValue2))] partial void Map(A source, B target);",
            "class A { public string StringValue { get; set; } public string StringValue2 { get; set; } public int IntValue { get; set; } }",
            "class B { public string StringValue { get; set; } public string StringValue2 { get; set; } public int IntValue { get; set; } }");

        TestHelper.GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(new(DiagnosticDescriptors.SourcePropertyNotMapped, "The property StringValue2 on the mapping source type A is not mapped to any property on the mapping target type B"))
            .HaveDiagnostic(new(DiagnosticDescriptors.MappingSourcePropertyNotFound, "Property StringValue on source type A was not found"))
            .HaveSingleMethodBody("target.IntValue = source.IntValue;");
    }

    [Fact]
    public void WithIgnoredSourcePropertyShouldIgnoreAndGenerateDiagnostics()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapperIgnoreSource(nameof(A.IntValue))] partial B Map(A source);",
            "class A { public string StringValue { get; set; } public int IntValue { get; set; } }",
            "class B { public string StringValue { get; set; }  public int IntValue { get; set; } }");

        TestHelper.GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(new(DiagnosticDescriptors.MappingSourcePropertyNotFound, "Property IntValue on source type A was not found"))
            .HaveSingleMethodBody(
                """
                var target = new B();
                target.StringValue = source.StringValue;
                return target;
                """);
    }

    [Fact]
    public void WithIgnoredTargetPropertyShouldIgnoreAndGenerateDiagnostics()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapperIgnoreTarget(nameof(B.IntValue))] partial B Map(A source);",
            "class A { public string StringValue { get; set; } public int IntValue { get; set; } }",
            "class B { public string StringValue { get; set; }  public int IntValue { get; set; } }");

        TestHelper.GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(new(DiagnosticDescriptors.SourcePropertyNotMapped, "The property IntValue on the mapping source type A is not mapped to any property on the mapping target type B"))
            .HaveSingleMethodBody(
                """
                var target = new B();
                target.StringValue = source.StringValue;
                return target;
                """);
    }

    [Fact]
    public void WithNotFoundIgnoredTargetPropertyShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapperIgnoreTarget(\"not_found\")] partial B Map(A source);",
            "class A { }",
            "class B { }");

        TestHelper.GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(new(DiagnosticDescriptors.IgnoredTargetPropertyNotFound, "Ignored target property not_found on B was not found"))
            .HaveSingleMethodBody(
                """
                var target = new B();
                return target;
                """);
    }

    [Fact]
    public void WithNotFoundIgnoredSourcePropertyShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapperIgnoreSource(\"not_found\")] partial B Map(A source);",
            "class A { }",
            "class B { }");

        TestHelper.GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(new(DiagnosticDescriptors.IgnoredSourcePropertyNotFound, "Ignored source property not_found on A was not found"))
            .HaveSingleMethodBody(
                """
                var target = new B();
                return target;
                """);
    }

    [Fact]
    public void WithNotFoundIgnoredObsoleteTargetAttributePropertyShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapperIgnore(\"not_found\")] partial B Map(A source);",
            "class A { }",
            "class B { }");

        TestHelper.GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(new(DiagnosticDescriptors.IgnoredTargetPropertyNotFound, "Ignored target property not_found on B was not found"))
            .HaveSingleMethodBody(
                """
                var target = new B();
                return target;
                """);
    }

    [Fact]
    public void WithObsoleteIgnoredTargetPropertyAttributeShouldIgnoreAndGenerateDiagnostics()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapperIgnore(nameof(B.IntValue))] partial B Map(A source);",
            "class A { public string StringValue { get; set; } public int IntValue { get; set; } }",
            "class B { public string StringValue { get; set; }  public int IntValue { get; set; } }");

        TestHelper.GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(new(DiagnosticDescriptors.SourcePropertyNotMapped, "The property IntValue on the mapping source type A is not mapped to any property on the mapping target type B"))
            .HaveSingleMethodBody(
                """
                var target = new B();
                target.StringValue = source.StringValue;
                return target;
                """);
    }
}
