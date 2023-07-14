using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class EnumIgnoreTest
{
    [Fact]
    public void ByValueWithIgnoredSourceValueShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapperIgnoreSourceValue(E1.D), MapEnum(EnumMappingStrategy.ByValue)] partial E2 ToE1(E1 source);",
            "enum E1 {A, B, C, D = 100, E}",
            "enum E2 {AA, BB, CC}"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.SourceEnumValueNotMapped, "Enum member E (101) on E1 not found on target enum E2")
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody("return (global::E2)source;");
    }

    [Fact]
    public void ByValueWithIgnoredTargetValueShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapperIgnoreTargetValue(E2.DD), MapEnum(EnumMappingStrategy.ByValue)] partial E2 ToE1(E1 source);",
            "enum E1 {A, B, C}",
            "enum E2 {AA, BB, CC, DD = 100, EE}"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.TargetEnumValueNotMapped, "Enum member EE (101) on E2 not found on source enum E1")
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody("return (global::E2)source;");
    }
}
