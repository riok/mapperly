using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class CtorTest
{
    [Fact]
    public void CtorCustomClass()
    {
        var source = TestSourceBuilder.Mapping(
            "string",
            "A",
            "class A { public A(string x) {} }");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return new global::A(source);");
    }

    [Fact]
    public void CtorCustomStruct()
    {
        var source = TestSourceBuilder.Mapping(
            "string",
            "A",
            "struct A { public A(string x) {} }");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return new global::A(source);");
    }

    [Fact]
    public void CtorMappingDisabledShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "string",
            TestSourceBuilderOptions.WithDisabledMappingConversion(MappingConversionType.ToStringMethod),
            "class A { public A(string x) {} }");
        TestHelper.GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(new(DiagnosticDescriptors.CouldNotCreateMapping));
    }
}
