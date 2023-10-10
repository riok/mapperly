using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class CtorTest
{
    [Fact]
    public void CtorCustomClass()
    {
        var source = TestSourceBuilder.Mapping("string", "A", "class A { public A(string x) {} }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return new global::A(source);");
    }

    [Fact]
    public void CtorCustomStruct()
    {
        var source = TestSourceBuilder.Mapping("string", "A", "struct A { public A(string x) {} }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return new global::A(source);");
    }

    [Fact]
    public void PrimaryCtorCustomClass()
    {
        var source = TestSourceBuilder.Mapping("string", "A", "class A(string x) {}");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return new global::A(source);");
    }

    [Fact]
    public void CtorClassNullableSource()
    {
        var source = TestSourceBuilder.Mapping("int?", "A", "class A { public A(int x) {} }");
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                "return source == null ? throw new System.ArgumentNullException(nameof(source)) : new global::A(source.Value);"
            );
    }

    [Fact]
    public void CtorClassNullableParameter()
    {
        var source = TestSourceBuilder.Mapping("int?", "A", "class A { public A(int? x) {} }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return new global::A(source);");
    }

    [Fact]
    public void CtorClassNonNullSourceNullableParameter()
    {
        var source = TestSourceBuilder.Mapping("int", "A", "class A { public A(int? x) {} }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return new global::A(source);");
    }

    [Fact]
    public void CtorMappingDisabledShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "string",
            TestSourceBuilderOptions.WithDisabledMappingConversion(MappingConversionType.ToStringMethod),
            "class A { public A(string x) {} }"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.CouldNotCreateMapping,
                "Could not create mapping from A to string. Consider implementing the mapping manually."
            )
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void DeepCloneRecordShouldNotUseCtorMapping()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "A",
            TestSourceBuilderOptions.WithDeepCloning,
            "record A { public int Value { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::A();
                target.Value = source.Value;
                return target;
                """
            );
    }
}
