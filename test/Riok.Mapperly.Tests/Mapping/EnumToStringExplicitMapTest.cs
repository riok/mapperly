using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class EnumToStringExplicitMapTest
{
    [Fact]
    public void EnumToStringWithExplicitValue()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnumValue(E.e, \"str-e\")] public partial string ToStr(E source);",
            "public enum E {A = 100, B, C, d, e, E, f}"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveSingleMethodBody(
                """
                return source switch
                {
                    global::E.A => nameof(global::E.A),
                    global::E.B => nameof(global::E.B),
                    global::E.C => nameof(global::E.C),
                    global::E.d => nameof(global::E.d),
                    global::E.e => "str-e",
                    global::E.E => nameof(global::E.E),
                    global::E.f => nameof(global::E.f),
                    _ => source.ToString(),
                };
                """
            )
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void EnumToStringWithExplicitValueMultipleSourcesToOneString()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnumValue(E.e, \"str-e\"), MapEnumValue(E.E, \"str-e\")] public partial string ToStr(E source);",
            "public enum E {A = 100, B, C, d, e, E, f}"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveSingleMethodBody(
                """
                return source switch
                {
                    global::E.A => nameof(global::E.A),
                    global::E.B => nameof(global::E.B),
                    global::E.C => nameof(global::E.C),
                    global::E.d => nameof(global::E.d),
                    global::E.e => "str-e",
                    global::E.E => "str-e",
                    global::E.f => nameof(global::E.f),
                    _ => source.ToString(),
                };
                """
            )
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void EnumToStringWithExplicitValueDuplicatedSource()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnumValue(E.e, \"str1-e\"), MapEnumValue(E.e, \"str2-e\")] public partial string ToStr(E source);",
            "public enum E {A = 100, B, C, d, e, E, f}"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveSingleMethodBody(
                """
                return source switch
                {
                    global::E.A => nameof(global::E.A),
                    global::E.B => nameof(global::E.B),
                    global::E.C => nameof(global::E.C),
                    global::E.d => nameof(global::E.d),
                    global::E.e => "str1-e",
                    global::E.E => nameof(global::E.E),
                    global::E.f => nameof(global::E.f),
                    _ => source.ToString(),
                };
                """
            )
            .HaveDiagnostic(
                DiagnosticDescriptors.EnumSourceValueDuplicated,
                "Enum source value E.e is specified multiple times, a source enum value may only be specified once"
            )
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void EnumToStringWithExplicitValueSourceEnumTypeMismatch()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnumValue(E2.A, \"str-A\"))] public partial string ToStr(E1 source);",
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
                    global::E1.A => nameof(global::E1.A),
                    global::E1.B => nameof(global::E1.B),
                    global::E1.C => nameof(global::E1.C),
                    global::E1.d => nameof(global::E1.d),
                    global::E1.e => nameof(global::E1.e),
                    global::E1.E => nameof(global::E1.E),
                    global::E1.f => nameof(global::E1.f),
                    _ => source.ToString(),
                };
                """
            )
            .HaveDiagnostic(
                DiagnosticDescriptors.SourceEnumValueDoesNotMatchSourceEnumType,
                "Enum member E2.A (0) on E2 does not match type of source enum E1"
            )
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void EnumToStringWithExplicitValueTargetTypeMismatch()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnumValue(E1.A, E1.A))] public partial string ToStr(E1 source);",
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
                    global::E1.A => nameof(global::E1.A),
                    global::E1.B => nameof(global::E1.B),
                    global::E1.C => nameof(global::E1.C),
                    global::E1.d => nameof(global::E1.d),
                    global::E1.e => nameof(global::E1.e),
                    global::E1.E => nameof(global::E1.E),
                    global::E1.f => nameof(global::E1.f),
                    _ => source.ToString(),
                };
                """
            )
            .HaveDiagnostic(
                DiagnosticDescriptors.EnumExplicitMappingTargetNotString,
                "The target of the explicit mapping from an enum to a string is not of type string"
            )
            .HaveAssertedAllDiagnostics();
    }
}
