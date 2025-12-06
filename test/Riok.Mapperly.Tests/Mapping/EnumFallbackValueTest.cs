using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class EnumFallbackValueTest
{
    [Fact]
    public void EnumByNameWithFallbackShouldSwitch()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByName, IgnoreCase = true, FallbackValue = E2.Unknown)] partial E2 ToE1(E1 source);",
            "enum E1 {A, B, C, D}",
            "enum E2 {Unknown = -1, A = 100, B, C, d}"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return source switch
                {
                    global::E1.A => global::E2.A,
                    global::E1.B => global::E2.B,
                    global::E1.C => global::E2.C,
                    global::E1.D => global::E2.d,
                    _ => global::E2.Unknown,
                };
                """
            );
    }

    [Fact]
    public void EnumByValueCheckDefinedWithFallback()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByValueCheckDefined, FallbackValue = E2.Unknown)] partial E2 ToE1(E1 source);",
            "enum E1 {A, B, C}",
            "enum E2 {Unknown = -1, A, B, C}"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                "return (global::E2)source is global::E2.Unknown or global::E2.A or global::E2.B or global::E2.C ? (global::E2)source : global::E2.Unknown;"
            );
    }

    [Fact]
    public void FlagsEnumByValueCheckDefinedWithFallback()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByValueCheckDefined, FallbackValue = E2.Unknown)] partial E2 ToE1(E1 source);",
            "enum E1 {A = 1 << 0, B = 1 << 1, C = 1 << 2}",
            "[Flags] enum E2 {Unknown = -1, A = 1 << 0, B = 1 << 1, C = 1 << 2}"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                "return (global::E2)source == ((global::E2)source & (global::E2.Unknown | global::E2.A | global::E2.B | global::E2.C)) ? (global::E2)source : global::E2.Unknown;"
            );
    }

    [Fact]
    public void EnumByValueWithFallbackShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByValue, FallbackValue = E2.Unknown)] partial E2 ToE1(E1 source);",
            "enum E1 {A, B, C, D, E}",
            "enum E2 {Unknown = -1, A, B, C, D, E}"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveSingleMethodBody(
                "return (global::E2)source is global::E2.Unknown or global::E2.A or global::E2.B or global::E2.C or global::E2.D or global::E2.E ? (global::E2)source : global::E2.Unknown;"
            )
            .HaveDiagnostic(
                DiagnosticDescriptors.EnumFallbackValueRequiresByValueCheckDefinedStrategy,
                "Enum fallback values are only supported for the ByName and ByValueCheckDefined strategies, but not for the ByValue strategy"
            )
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void StringToEnumFallbackValueShouldSwitchIgnoreCase()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByName, IgnoreCase = true, FallbackValue = E1.Unknown)] partial E1 ToE1(string source);",
            "enum E1 {Unknown = -1, A, B, C}"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return source switch
                {
                    {} s when s.Equals(nameof(global::E1.A), global::System.StringComparison.OrdinalIgnoreCase) => global::E1.A,
                    {} s when s.Equals(nameof(global::E1.B), global::System.StringComparison.OrdinalIgnoreCase) => global::E1.B,
                    {} s when s.Equals(nameof(global::E1.C), global::System.StringComparison.OrdinalIgnoreCase) => global::E1.C,
                    _ => global::E1.Unknown,
                };
                """
            );
    }

    [Fact]
    public void StringToEnumShouldSwitch()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByName, FallbackValue = E1.Unknown)] partial E1 ToE1(string source);",
            "enum E1 {Unknown = -1, A, B, C}"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return source switch
                {
                    nameof(global::E1.A) => global::E1.A,
                    nameof(global::E1.B) => global::E1.B,
                    nameof(global::E1.C) => global::E1.C,
                    _ => global::E1.Unknown,
                };
                """
            );
    }

    [Fact]
    public void EnumToStringFallbackValueShouldSwitch()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByName, FallbackValue = \"_unknown\")] partial string ToE1(E1 source);",
            "enum E1 {Unknown = -1, A, B, C}"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return source switch
                {
                    global::E1.Unknown => nameof(global::E1.Unknown),
                    global::E1.A => nameof(global::E1.A),
                    global::E1.B => nameof(global::E1.B),
                    global::E1.C => nameof(global::E1.C),
                    _ => "_unknown",
                };
                """
            );
    }

    [Fact]
    public void EnumToStringWithNamingStrategyFallbackValueShouldSwitch()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByName, NamingStrategy = EnumNamingStrategy.CamelCase, FallbackValue = \"unknown\")] partial string ToE1(E1 source);",
            "enum E1 {Unknown = -1, A, B, C}"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return source switch
                {
                    global::E1.A => "a",
                    global::E1.B => "b",
                    global::E1.C => "c",
                    _ => "unknown",
                };
                """
            );
    }
}
