using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class ToStringTest
{
    [Fact]
    public void CustomClassToString()
    {
        var source = TestSourceBuilder.Mapping("A", "string", "class A {}");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source.ToString();");
    }

    [Fact]
    public void CustomStructToString()
    {
        var source = TestSourceBuilder.Mapping("A", "string", "struct A {}");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source.ToString();");
    }

    [Fact]
    public void BuiltInStructToString()
    {
        var source = TestSourceBuilder.Mapping("DateTime", "string");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source.ToString();");
    }

    [Fact]
    public void ObjectToString()
    {
        var source = TestSourceBuilder.Mapping("object", "string");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source.ToString();");
    }

    [Fact]
    public void ToStringMappingDisabledShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "string",
            TestSourceBuilderOptions.WithDisabledMappingConversion(MappingConversionType.ToStringMethod),
            "class A {}"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.CouldNotCreateMapping)
            .HaveAssertedAllDiagnostics();
    }
}
