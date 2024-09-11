using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class StringToEnumExplicitMapTest
{
    [Fact]
    public void EnumFromStringWithExplicitValue()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnumValue(\"str-e\", E.e)] public partial E FromStr(string source);",
            "public enum E {A = 100, B, C, d, e, E, f}"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveSingleMethodBody(
                """
                return source switch
                {
                    nameof(global::E.A) => global::E.A,
                    nameof(global::E.B) => global::E.B,
                    nameof(global::E.C) => global::E.C,
                    nameof(global::E.d) => global::E.d,
                    "str-e" => global::E.e,
                    nameof(global::E.E) => global::E.E,
                    nameof(global::E.f) => global::E.f,
                    _ => System.Enum.Parse<global::E>(source, false),
                };
                """
            )
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void EnumFromStringWithExplicitValueMultipleSourcesToOneEnum()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnumValue(\"str-e1\", E.e), MapEnumValue(\"str-e2\", E.e)] public partial E FromStr(string source);",
            "public enum E {A = 100, B, C, d, e, E, f}"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveSingleMethodBody(
                """
                return source switch
                {
                    nameof(global::E.A) => global::E.A,
                    nameof(global::E.B) => global::E.B,
                    nameof(global::E.C) => global::E.C,
                    nameof(global::E.d) => global::E.d,
                    "str-e1" => global::E.e,
                    "str-e2" => global::E.e,
                    nameof(global::E.E) => global::E.E,
                    nameof(global::E.f) => global::E.f,
                    _ => System.Enum.Parse<global::E>(source, false),
                };
                """
            )
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void EnumToStringWithExplicitValueDuplicatedSource()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnumValue(\"str-e\", E.e), MapEnumValue(\"str-e\", E.E)] public partial E FromStr(string source);",
            "public enum E {A = 100, B, C, d, e, E, f}"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveSingleMethodBody(
                """
                return source switch
                {
                    nameof(global::E.A) => global::E.A,
                    nameof(global::E.B) => global::E.B,
                    nameof(global::E.C) => global::E.C,
                    nameof(global::E.d) => global::E.d,
                    "str-e" => global::E.e,
                    nameof(global::E.E) => global::E.E,
                    nameof(global::E.f) => global::E.f,
                    _ => System.Enum.Parse<global::E>(source, false),
                };
                """
            )
            .HaveDiagnostic(
                DiagnosticDescriptors.StringSourceValueDuplicated,
                "String source value \"str-e\" is specified multiple times, a source string value may only be specified once"
            )
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void EnumToStringWithExplicitValueSourceEnumTypeMismatch()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnumValue(\"str-A\", E2.A))] public partial E1 FromStr(string source);",
            "public enum E1 {A = 100, B, C, d, e, E, f}",
            "public enum E2 {A}"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveSingleMethodBody(
                """
                return source switch
                {
                    nameof(global::E1.A) => global::E1.A,
                    nameof(global::E1.B) => global::E1.B,
                    nameof(global::E1.C) => global::E1.C,
                    nameof(global::E1.d) => global::E1.d,
                    nameof(global::E1.e) => global::E1.e,
                    nameof(global::E1.E) => global::E1.E,
                    nameof(global::E1.f) => global::E1.f,
                    _ => System.Enum.Parse<global::E1>(source, false),
                };
                """
            )
            .HaveDiagnostic(
                DiagnosticDescriptors.TargetEnumValueDoesNotMatchTargetEnumType,
                "Enum member E2.A (0) on E2 does not match type of target enum E1"
            )
            .HaveAssertedAllDiagnostics();
    }
}
