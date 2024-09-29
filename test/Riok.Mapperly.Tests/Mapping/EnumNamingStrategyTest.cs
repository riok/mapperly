using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class EnumNamingStrategyTest
{
    [Fact]
    public void EnumToStringWithMemberNameNamingStrategy()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByName, NamingStrategy = EnumNamingStrategy.MemberName)] public partial string ToStr(E source);",
            "public enum E {Abc, BcD, C1D, DEf, EFG, FG1, Gh1, Hi_J}"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return source switch
                {
                    global::E.Abc => nameof(global::E.Abc),
                    global::E.BcD => nameof(global::E.BcD),
                    global::E.C1D => nameof(global::E.C1D),
                    global::E.DEf => nameof(global::E.DEf),
                    global::E.EFG => nameof(global::E.EFG),
                    global::E.FG1 => nameof(global::E.FG1),
                    global::E.Gh1 => nameof(global::E.Gh1),
                    global::E.Hi_J => nameof(global::E.Hi_J),
                    _ => source.ToString(),
                };
                """
            );
    }

    [Fact]
    public void EnumToStringWithCamelCaseNamingStrategy()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByName, NamingStrategy = EnumNamingStrategy.CamelCase)] public partial string ToStr(E source);",
            "public enum E {Abc, BcD, C1D, DEf, EFG, FG1, Gh1, Hi_J}"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return source switch
                {
                    global::E.Abc => "abc",
                    global::E.BcD => "bcD",
                    global::E.C1D => "c1D",
                    global::E.DEf => "dEf",
                    global::E.EFG => "efg",
                    global::E.FG1 => "fg1",
                    global::E.Gh1 => "gh1",
                    global::E.Hi_J => "hiJ",
                    _ => source.ToString(),
                };
                """
            );
    }

    [Fact]
    public void EnumToStringWithPascalCaseNamingStrategy()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByName, NamingStrategy = EnumNamingStrategy.PascalCase)] public partial string ToStr(E source);",
            "public enum E {Abc, BcD, C1D, DEf, EFG, FG1, Gh1, Hi_J}"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return source switch
                {
                    global::E.Abc => "Abc",
                    global::E.BcD => "BcD",
                    global::E.C1D => "C1D",
                    global::E.DEf => "DEf",
                    global::E.EFG => "Efg",
                    global::E.FG1 => "Fg1",
                    global::E.Gh1 => "Gh1",
                    global::E.Hi_J => "HiJ",
                    _ => source.ToString(),
                };
                """
            );
    }

    [Fact]
    public void EnumToStringWithSnakeCaseNamingStrategy()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByName, NamingStrategy = EnumNamingStrategy.SnakeCase)] public partial string ToStr(E source);",
            "public enum E {Abc, BcD, C1D, DEf, EFG, FG1, Gh1, Hi_J}"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return source switch
                {
                    global::E.Abc => "abc",
                    global::E.BcD => "bc_d",
                    global::E.C1D => "c1_d",
                    global::E.DEf => "d_ef",
                    global::E.EFG => "efg",
                    global::E.FG1 => "fg1",
                    global::E.Gh1 => "gh1",
                    global::E.Hi_J => "hi_j",
                    _ => source.ToString(),
                };
                """
            );
    }

    [Fact]
    public void EnumToStringWithUpperSnakeCaseNamingStrategy()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByName, NamingStrategy = EnumNamingStrategy.UpperSnakeCase)] public partial string ToStr(E source);",
            "public enum E {Abc, BcD, C1D, DEf, EFG, FG1, Gh1, Hi_J}"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return source switch
                {
                    global::E.Abc => "ABC",
                    global::E.BcD => "BC_D",
                    global::E.C1D => "C1_D",
                    global::E.DEf => "D_EF",
                    global::E.EFG => "EFG",
                    global::E.FG1 => "FG1",
                    global::E.Gh1 => "GH1",
                    global::E.Hi_J => "HI_J",
                    _ => source.ToString(),
                };
                """
            );
    }

    [Fact]
    public void EnumToStringWithKebabCaseNamingStrategy()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByName, NamingStrategy = EnumNamingStrategy.KebabCase)] public partial string ToStr(E source);",
            "public enum E {Abc, BcD, C1D, DEf, EFG, FG1, Gh1, Hi_J}"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return source switch
                {
                    global::E.Abc => "abc",
                    global::E.BcD => "bc-d",
                    global::E.C1D => "c1-d",
                    global::E.DEf => "d-ef",
                    global::E.EFG => "efg",
                    global::E.FG1 => "fg1",
                    global::E.Gh1 => "gh1",
                    global::E.Hi_J => "hi-j",
                    _ => source.ToString(),
                };
                """
            );
    }

    [Fact]
    public void EnumToStringWithUpperKebabCaseNamingStrategy()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByName, NamingStrategy = EnumNamingStrategy.UpperKebabCase)] public partial string ToStr(E source);",
            "public enum E {Abc, BcD, C1D, DEf, EFG, FG1, Gh1, Hi_J}"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return source switch
                {
                    global::E.Abc => "ABC",
                    global::E.BcD => "BC-D",
                    global::E.C1D => "C1-D",
                    global::E.DEf => "D-EF",
                    global::E.EFG => "EFG",
                    global::E.FG1 => "FG1",
                    global::E.Gh1 => "GH1",
                    global::E.Hi_J => "HI-J",
                    _ => source.ToString(),
                };
                """
            );
    }

    [Fact]
    public void EnumToStringWithComponentModelDescriptionAttributeNamingStrategy()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByName, NamingStrategy = EnumNamingStrategy.ComponentModelDescriptionAttribute)] public partial string ToStr(E source);",
            """
            public enum E
            {
                [System.ComponentModel.Description("A1")] A,
                [System.ComponentModel.Description("")] B,
                [System.ComponentModel.Description] C,
                D,
            }
            """
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics)
            .Should()
            .HaveSingleMethodBody(
                """
                return source switch
                {
                    global::E.A => "A1",
                    global::E.B => "",
                    global::E.C => "C",
                    global::E.D => "D",
                    _ => source.ToString(),
                };
                """
            )
            .HaveDiagnostics(
                DiagnosticDescriptors.EnumNamingAttributeMissing,
                "The DescriptionAttribute to build the name of the enum member C (2) is missing",
                "The DescriptionAttribute to build the name of the enum member D (3) is missing"
            )
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void EnumToStringWithSerializationEnumMemberAttributeNamingStrategy()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByName, NamingStrategy = EnumNamingStrategy.SerializationEnumMemberAttribute)] public partial string ToStr(E source);",
            """
            public enum E
            {
                [System.Runtime.Serialization.EnumMember(Value = "A1")] A,
                [System.Runtime.Serialization.EnumMember(Value = "")] B,
                [System.Runtime.Serialization.EnumMember(Value = null)] C,
                [System.Runtime.Serialization.EnumMember)] D,
                E,
            }
            """
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics)
            .Should()
            .HaveSingleMethodBody(
                """
                return source switch
                {
                    global::E.A => "A1",
                    global::E.B => "",
                    global::E.C => "C",
                    global::E.D => "D",
                    global::E.E => "E",
                    _ => source.ToString(),
                };
                """
            )
            .HaveDiagnostics(
                DiagnosticDescriptors.EnumNamingAttributeMissing,
                "The EnumMemberAttribute to build the name of the enum member C (2) is missing",
                "The EnumMemberAttribute to build the name of the enum member D (3) is missing",
                "The EnumMemberAttribute to build the name of the enum member E (4) is missing"
            )
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void EnumFromStringWithMemberNameNamingStrategy()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByName, NamingStrategy = EnumNamingStrategy.MemberName)] public partial E ToEnum(string source);",
            "public enum E {Abc, BcD, C1D, DEf, EFG, FG1, Gh1, Hi_J}"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return source switch
                {
                    nameof(global::E.Abc) => global::E.Abc,
                    nameof(global::E.BcD) => global::E.BcD,
                    nameof(global::E.C1D) => global::E.C1D,
                    nameof(global::E.DEf) => global::E.DEf,
                    nameof(global::E.EFG) => global::E.EFG,
                    nameof(global::E.FG1) => global::E.FG1,
                    nameof(global::E.Gh1) => global::E.Gh1,
                    nameof(global::E.Hi_J) => global::E.Hi_J,
                    _ => System.Enum.Parse<global::E>(source, false),
                };
                """
            );
    }

    [Fact]
    public void EnumFromStringWithCamelCaseNamingStrategy()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByName, NamingStrategy = EnumNamingStrategy.CamelCase)] public partial E ToEnum(string source);",
            "public enum E {Abc, BcD, C1D, DEf, EFG, FG1, Gh1, Hi_J}"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return source switch
                {
                    "abc" => global::E.Abc,
                    "bcD" => global::E.BcD,
                    "c1D" => global::E.C1D,
                    "dEf" => global::E.DEf,
                    "efg" => global::E.EFG,
                    "fg1" => global::E.FG1,
                    "gh1" => global::E.Gh1,
                    "hiJ" => global::E.Hi_J,
                    _ => System.Enum.Parse<global::E>(source, false),
                };
                """
            );
    }

    [Fact]
    public void EnumFromStringWithPascalCaseNamingStrategy()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByName, NamingStrategy = EnumNamingStrategy.PascalCase)] public partial E ToEnum(string source);",
            "public enum E {Abc, BcD, C1D, DEf, EFG, FG1, Gh1, Hi_J}"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return source switch
                {
                    "Abc" => global::E.Abc,
                    "BcD" => global::E.BcD,
                    "C1D" => global::E.C1D,
                    "DEf" => global::E.DEf,
                    "Efg" => global::E.EFG,
                    "Fg1" => global::E.FG1,
                    "Gh1" => global::E.Gh1,
                    "HiJ" => global::E.Hi_J,
                    _ => System.Enum.Parse<global::E>(source, false),
                };
                """
            );
    }

    [Fact]
    public void EnumFromStringWithSnakeCaseNamingStrategy()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByName, NamingStrategy = EnumNamingStrategy.SnakeCase)] public partial E ToEnum(string source);",
            "public enum E {Abc, BcD, C1D, DEf, EFG, FG1, Gh1, Hi_J}"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return source switch
                {
                    "abc" => global::E.Abc,
                    "bc_d" => global::E.BcD,
                    "c1_d" => global::E.C1D,
                    "d_ef" => global::E.DEf,
                    "efg" => global::E.EFG,
                    "fg1" => global::E.FG1,
                    "gh1" => global::E.Gh1,
                    "hi_j" => global::E.Hi_J,
                    _ => System.Enum.Parse<global::E>(source, false),
                };
                """
            );
    }

    [Fact]
    public void EnumFromStringWithUpperSnakeCaseNamingStrategy()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByName, NamingStrategy = EnumNamingStrategy.UpperSnakeCase)] public partial E ToEnum(string source);",
            "public enum E {Abc, BcD, C1D, DEf, EFG, FG1, Gh1, Hi_J}"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return source switch
                {
                    "ABC" => global::E.Abc,
                    "BC_D" => global::E.BcD,
                    "C1_D" => global::E.C1D,
                    "D_EF" => global::E.DEf,
                    "EFG" => global::E.EFG,
                    "FG1" => global::E.FG1,
                    "GH1" => global::E.Gh1,
                    "HI_J" => global::E.Hi_J,
                    _ => System.Enum.Parse<global::E>(source, false),
                };
                """
            );
    }

    [Fact]
    public void EnumFromStringWithKebabCaseNamingStrategy()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByName, NamingStrategy = EnumNamingStrategy.KebabCase)] public partial E ToEnum(string source);",
            "public enum E {Abc, BcD, C1D, DEf, EFG, FG1, Gh1, Hi_J}"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return source switch
                {
                    "abc" => global::E.Abc,
                    "bc-d" => global::E.BcD,
                    "c1-d" => global::E.C1D,
                    "d-ef" => global::E.DEf,
                    "efg" => global::E.EFG,
                    "fg1" => global::E.FG1,
                    "gh1" => global::E.Gh1,
                    "hi-j" => global::E.Hi_J,
                    _ => System.Enum.Parse<global::E>(source, false),
                };
                """
            );
    }

    [Fact]
    public void EnumFromStringWithUpperKebabCaseNamingStrategy()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByName, NamingStrategy = EnumNamingStrategy.UpperKebabCase)] public partial E ToEnum(string source);",
            "public enum E {Abc, BcD, C1D, DEf, EFG, FG1, Gh1, Hi_J}"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return source switch
                {
                    "ABC" => global::E.Abc,
                    "BC-D" => global::E.BcD,
                    "C1-D" => global::E.C1D,
                    "D-EF" => global::E.DEf,
                    "EFG" => global::E.EFG,
                    "FG1" => global::E.FG1,
                    "GH1" => global::E.Gh1,
                    "HI-J" => global::E.Hi_J,
                    _ => System.Enum.Parse<global::E>(source, false),
                };
                """
            );
    }

    [Fact]
    public void EnumFromStringWithComponentModelDescriptionAttributeNamingStrategy()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByName, NamingStrategy = EnumNamingStrategy.ComponentModelDescriptionAttribute)] public partial E ToEnum(string source);",
            """
            public enum E
            {
                [System.ComponentModel.Description("A1")] A,
                [System.ComponentModel.Description("")] B,
                [System.ComponentModel.Description] C,
                D,
            }
            """
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics)
            .Should()
            .HaveSingleMethodBody(
                """
                return source switch
                {
                    "A1" => global::E.A,
                    "" => global::E.B,
                    "C" => global::E.C,
                    "D" => global::E.D,
                    _ => System.Enum.Parse<global::E>(source, false),
                };
                """
            )
            .HaveDiagnostics(
                DiagnosticDescriptors.EnumNamingAttributeMissing,
                "The DescriptionAttribute to build the name of the enum member C (2) is missing",
                "The DescriptionAttribute to build the name of the enum member D (3) is missing"
            )
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void EnumFromStringWithSerializationEnumMemberAttributeNamingStrategy()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByName, NamingStrategy = EnumNamingStrategy.SerializationEnumMemberAttribute)] public partial E ToEnum(string source);",
            """
            public enum E
            {
                [System.Runtime.Serialization.EnumMember(Value = "A1")] A,
                [System.Runtime.Serialization.EnumMember(Value = "")] B,
                [System.Runtime.Serialization.EnumMember(Value = null)] C,
                [System.Runtime.Serialization.EnumMember)] D,
                E,
            }
            """
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics)
            .Should()
            .HaveSingleMethodBody(
                """
                return source switch
                {
                    "A1" => global::E.A,
                    "" => global::E.B,
                    "C" => global::E.C,
                    "D" => global::E.D,
                    "E" => global::E.E,
                    _ => System.Enum.Parse<global::E>(source, false),
                };
                """
            )
            .HaveDiagnostics(
                DiagnosticDescriptors.EnumNamingAttributeMissing,
                "The EnumMemberAttribute to build the name of the enum member C (2) is missing",
                "The EnumMemberAttribute to build the name of the enum member D (3) is missing",
                "The EnumMemberAttribute to build the name of the enum member E (4) is missing"
            )
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void EnumToStringWithFallbackValue()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByName, NamingStrategy = EnumNamingStrategy.SnakeCase, FallbackValue = \"default\")] public partial string ToStr(E source);",
            "public enum E {Abc, BcD, C1D, DEf, EFG, FG1, Gh1, Hi_J}"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return source switch
                {
                    global::E.Abc => "abc",
                    global::E.BcD => "bc_d",
                    global::E.C1D => "c1_d",
                    global::E.DEf => "d_ef",
                    global::E.EFG => "efg",
                    global::E.FG1 => "fg1",
                    global::E.Gh1 => "gh1",
                    global::E.Hi_J => "hi_j",
                    _ => "default",
                };
                """
            );
    }

    [Fact]
    public void EnumFromStringWithFallbackValue()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByName, NamingStrategy = EnumNamingStrategy.SnakeCase, FallbackValue = E.DEf)] public partial E ToEnum(string source);",
            "public enum E {Abc, BcD, C1D, DEf, EFG, FG1, Gh1, Hi_J}"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return source switch
                {
                    "abc" => global::E.Abc,
                    "bc_d" => global::E.BcD,
                    "c1_d" => global::E.C1D,
                    "efg" => global::E.EFG,
                    "fg1" => global::E.FG1,
                    "gh1" => global::E.Gh1,
                    "hi_j" => global::E.Hi_J,
                    _ => global::E.DEf,
                };
                """
            );
    }

    [Fact]
    public void EnumFromStringWithAttributeNamingStrategyAndDuplicatedName()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByName, NamingStrategy = EnumNamingStrategy.ComponentModelDescriptionAttribute)] public partial E ToEnum(string source);",
            """
            public enum E
            {
                [System.ComponentModel.Description("A1")] A,
                [System.ComponentModel.Description("A1")] B,
                C,
            }
            """
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveSingleMethodBody(
                """
                return source switch
                {
                    "A1" => global::E.A,
                    "C" => global::E.C,
                    _ => System.Enum.Parse<global::E>(source, false),
                };
                """
            )
            .HaveDiagnostic(
                DiagnosticDescriptors.EnumStringSourceValueDuplicated,
                "String source value A1 is specified multiple times, a source string value may only be specified once"
            )
            .HaveDiagnostic(
                DiagnosticDescriptors.EnumNamingAttributeMissing,
                "The DescriptionAttribute to build the name of the enum member C (2) is missing"
            )
            .HaveAssertedAllDiagnostics();
    }
}
