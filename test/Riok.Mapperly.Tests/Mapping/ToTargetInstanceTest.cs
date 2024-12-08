using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class ToTargetInstanceTest
{
    [Fact]
    public void CustomClassToTarget()
    {
        var source = TestSourceBuilder.Mapping("A", "int", "class A { public int ToInt32() => 0; }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source.ToInt32();");
    }

    [Fact]
    public void CustomStructToTarget()
    {
        var source = TestSourceBuilder.Mapping("A", "int", "struct A { public int ToInt32() => 0; }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source.ToInt32();");
    }

    [Fact]
    public void CustomClassToTargetArray()
    {
        var source = TestSourceBuilder.Mapping("A", "string[]", "class A { public string[] ToStringArray() => [0]; }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source.ToStringArray();");
    }

    [Fact]
    public void CustomStructToTargetArray()
    {
        var source = TestSourceBuilder.Mapping("A", "float[]", "struct A { public float[] ToSingleArray() => [0]; }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source.ToSingleArray();");
    }

    [Fact]
    public void BuiltInStructToTargetArray()
    {
        var source = TestSourceBuilder.Mapping("Guid", "byte[]");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source.ToByteArray();");
    }

    [Fact]
    public void ToTargetMappingDisabledShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "int",
            TestSourceBuilderOptions.WithDisabledMappingConversion(MappingConversionType.ToTargetMethod),
            "class A { public int ToInt32() => 0; }"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.CouldNotCreateMapping)
            .HaveAssertedAllDiagnostics();
    }
}
