using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class EnumToEnumExplicitMapTest
{
    [Fact]
    public void EnumByNameWithExplicitValue()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnumValue(E2.e, E1.E), MapEnum(EnumMappingStrategy.ByName)] public partial E1 ToE1(E2 source);",
            "public enum E1 {A, B, C, D, E, f, F}",
            "public enum E2 {A = 100, B, C, d, e, E, f}"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveSingleMethodBody(
                """
                return source switch
                {
                    global::E2.A => global::E1.A,
                    global::E2.B => global::E1.B,
                    global::E2.C => global::E1.C,
                    global::E2.e => global::E1.E,
                    global::E2.E => global::E1.E,
                    global::E2.f => global::E1.f,
                    _ => throw new System.ArgumentOutOfRangeException(nameof(source), source, "The value of enum E2 is not supported"),
                };
                """
            )
            .HaveDiagnostic(DiagnosticDescriptors.SourceEnumValueNotMapped, "Enum member d (103) on E2 not found on target enum E1")
            .HaveDiagnostics(
                DiagnosticDescriptors.TargetEnumValueNotMapped,
                "Enum member D (3) on E1 not found on source enum E2",
                "Enum member F (6) on E1 not found on source enum E2"
            )
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void EnumByNameWithExplicitValueDuplicatedTarget()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnumValue(E2.e, E1.E), MapEnumValue(E2.d, E1.E), MapEnum(EnumMappingStrategy.ByName)] public partial E1 ToE1(E2 source);",
            "public enum E1 {A, B, C, D, E, f, F}",
            "public enum E2 {A = 100, B, C, d, e, E, f}"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveSingleMethodBody(
                """
                return source switch
                {
                    global::E2.A => global::E1.A,
                    global::E2.B => global::E1.B,
                    global::E2.C => global::E1.C,
                    global::E2.d => global::E1.E,
                    global::E2.e => global::E1.E,
                    global::E2.E => global::E1.E,
                    global::E2.f => global::E1.f,
                    _ => throw new System.ArgumentOutOfRangeException(nameof(source), source, "The value of enum E2 is not supported"),
                };
                """
            )
            .HaveDiagnostics(
                DiagnosticDescriptors.TargetEnumValueNotMapped,
                "Enum member D (3) on E1 not found on source enum E2",
                "Enum member F (6) on E1 not found on source enum E2"
            )
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void EnumByNameWithExplicitValueDuplicatedSource()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnumValue(E2.e, E1.E), MapEnumValue(E2.e, E1.F), MapEnum(EnumMappingStrategy.ByName)] public partial E1 ToE1(E2 source);",
            "public enum E1 {A, B, C, D, E, f, F}",
            "public enum E2 {A = 100, B, C, d, e, E, f}"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveSingleMethodBody(
                """
                return source switch
                {
                    global::E2.A => global::E1.A,
                    global::E2.B => global::E1.B,
                    global::E2.C => global::E1.C,
                    global::E2.e => global::E1.E,
                    global::E2.E => global::E1.E,
                    global::E2.f => global::E1.f,
                    _ => throw new System.ArgumentOutOfRangeException(nameof(source), source, "The value of enum E2 is not supported"),
                };
                """
            )
            .HaveDiagnostic(
                DiagnosticDescriptors.EnumSourceValueDuplicated,
                "Enum source value E2.e is specified multiple times, a source enum value may only be specified once"
            )
            .HaveDiagnostic(DiagnosticDescriptors.SourceEnumValueNotMapped, "Enum member d (103) on E2 not found on target enum E1")
            .HaveDiagnostics(
                DiagnosticDescriptors.TargetEnumValueNotMapped,
                "Enum member D (3) on E1 not found on source enum E2",
                "Enum member F (6) on E1 not found on source enum E2"
            )
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void EnumByNameWithExplicitValueSourceTypeMismatch()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnumValue(\"A\", E1.A), MapEnum(EnumMappingStrategy.ByName)] public partial E1 ToE1(E2 source);",
            "public enum E1 {A}",
            "public enum E2 {A}"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveSingleMethodBody(
                """
                return source switch
                {
                    global::E2.A => global::E1.A,
                    _ => throw new System.ArgumentOutOfRangeException(nameof(source), source, "The value of enum E2 is not supported"),
                };
                """
            )
            .HaveDiagnostic(DiagnosticDescriptors.MapValueTypeMismatch, "Cannot assign constant value \"A\" of type string to E2")
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void EnumByNameWithExplicitValueTargetTypeMismatch()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnumValue(E2.A, \"A\"), MapEnum(EnumMappingStrategy.ByName)] public partial E1 ToE1(E2 source);",
            "public enum E1 {A}",
            "public enum E2 {A}"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveSingleMethodBody(
                """
                return source switch
                {
                    global::E2.A => global::E1.A,
                    _ => throw new System.ArgumentOutOfRangeException(nameof(source), source, "The value of enum E2 is not supported"),
                };
                """
            )
            .HaveDiagnostic(DiagnosticDescriptors.MapValueTypeMismatch, "Cannot assign constant value \"A\" of type string to E1")
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void EnumByNameWithExplicitValueTargetEnumTypeMismatch()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnumValue(E2.A, E2.A), MapEnum(EnumMappingStrategy.ByName)] public partial E1 ToE1(E2 source);",
            "public enum E1 {A}",
            "public enum E2 {A}"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveSingleMethodBody(
                """
                return source switch
                {
                    global::E2.A => global::E1.A,
                    _ => throw new System.ArgumentOutOfRangeException(nameof(source), source, "The value of enum E2 is not supported"),
                };
                """
            )
            .HaveDiagnostic(
                DiagnosticDescriptors.TargetEnumValueDoesNotMatchTargetEnumType,
                "Enum member E2.A (0) on E2 does not match type of target enum E1"
            )
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void EnumByNameWithExplicitValueSourceEnumTypeMismatch()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnumValue(E1.A, E1.A), MapEnum(EnumMappingStrategy.ByName)] public partial E1 ToE1(E2 source);",
            "public enum E1 {A}",
            "public enum E2 {A}"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveSingleMethodBody(
                """
                return source switch
                {
                    global::E2.A => global::E1.A,
                    _ => throw new System.ArgumentOutOfRangeException(nameof(source), source, "The value of enum E2 is not supported"),
                };
                """
            )
            .HaveDiagnostic(
                DiagnosticDescriptors.SourceEnumValueDoesNotMatchSourceEnumType,
                "Enum member E1.A (0) on E1 does not match type of source enum E2"
            )
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void EnumByValueWithExplicitValue()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnumValue(E2.X, E1.XX), MapEnum(EnumMappingStrategy.ByValue)] public partial E1 Map(E2 source);",
            "public enum E1 {A, B, C, XX = 100}",
            "public enum E2 {A, B, C, X = 200}"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return source switch
                {
                    global::E2.X => global::E1.XX,
                    _ => (global::E1)source,
                };
                """
            );
    }

    [Fact]
    public void EnumByValueWithExplicitValueSameSourceAndTargetValueShouldBeCasted()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnumValue(E2.X, E1.XX), MapEnumValue(E2.Z, E1.ZZ), MapEnum(EnumMappingStrategy.ByValue)] public partial E1 Map(E2 source);",
            "public enum E1 {A, B, C, XX = 100, ZZ = 300}",
            "public enum E2 {A, B, C, X = 100, Z = 200}"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return source switch
                {
                    global::E2.Z => global::E1.ZZ,
                    _ => (global::E1)source,
                };
                """
            );
    }

    [Fact]
    public void EnumByValueWithExplicitValueDuplicatedTarget()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnumValue(E2.Y, E1.YY), MapEnumValue(E2.Z, E1.YY), MapEnum(EnumMappingStrategy.ByValue)] public partial E1 Map(E2 source);",
            "public enum E1 {A, B, C, YY = 100}",
            "public enum E2 {A, B, C, Y = 200, Z = 300}"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return source switch
                {
                    global::E2.Y => global::E1.YY,
                    global::E2.Z => global::E1.YY,
                    _ => (global::E1)source,
                };
                """
            );
    }

    [Fact]
    public void EnumByValueWithExplicitValueDuplicatedSource()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnumValue(E2.X, E1.XX), MapEnumValue(E2.X, E1.YY), MapEnum(EnumMappingStrategy.ByValue)] public partial E1 Map(E2 source);",
            "public enum E1 {A, B, C, XX = 100, YY = 200}",
            "public enum E2 {A, B, C, X = 200}"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveSingleMethodBody(
                """
                return source switch
                {
                    global::E2.X => global::E1.XX,
                    _ => (global::E1)source,
                };
                """
            )
            .HaveDiagnostic(
                DiagnosticDescriptors.EnumSourceValueDuplicated,
                "Enum source value E2.X is specified multiple times, a source enum value may only be specified once"
            )
            .HaveDiagnostic(DiagnosticDescriptors.TargetEnumValueNotMapped, "Enum member YY (200) on E1 not found on source enum E2")
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void EnumByValueWithExplicitValueSourceTypeMismatch()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnumValue(E1.XX, E1.XX), MapEnum(EnumMappingStrategy.ByValue)] public partial E1 Map(E2 source);",
            "public enum E1 {A, B, C, XX = 100}",
            "public enum E2 {A, B, C}"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveSingleMethodBody("return (global::E1)source;")
            .HaveDiagnostic(
                DiagnosticDescriptors.SourceEnumValueDoesNotMatchSourceEnumType,
                "Enum member E1.XX (100) on E1 does not match type of source enum E2"
            )
            .HaveDiagnostic(DiagnosticDescriptors.TargetEnumValueNotMapped, "Enum member XX (100) on E1 not found on source enum E2")
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void EnumByValueWithExplicitValueTargetTypeMismatch()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnumValue(E2.XX, E2.XX), MapEnum(EnumMappingStrategy.ByValue)] public partial E1 Map(E2 source);",
            "public enum E1 {A, B, C}",
            "public enum E2 {A, B, C, XX = 100}"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveSingleMethodBody("return (global::E1)source;")
            .HaveDiagnostic(
                DiagnosticDescriptors.TargetEnumValueDoesNotMatchTargetEnumType,
                "Enum member E2.XX (100) on E2 does not match type of target enum E1"
            )
            .HaveDiagnostic(DiagnosticDescriptors.SourceEnumValueNotMapped, "Enum member XX (100) on E2 not found on target enum E1")
            .HaveAssertedAllDiagnostics();
    }
}
