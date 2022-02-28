namespace Riok.Mapperly.Tests.Mapping;

[UsesVerify]
public class EnumTest
{
    [Fact]
    public void EnumToOtherEnumShouldCast()
    {
        var source = TestSourceBuilder.Mapping(
            "E2",
            "E1",
            "enum E1 {A, B, C}",
            "enum E2 {A, B, C}");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return (E1)source;");
    }

    [Fact]
    public void EnumToSameEnumShouldAssign()
    {
        var source = TestSourceBuilder.Mapping(
            "E1",
            "E1",
            "enum E1 {A, B, C}");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return source;");
    }

    [Fact]
    public void EnumToOtherEnumTypeShouldCast()
    {
        var source = TestSourceBuilder.Mapping(
            "E1",
            "E2",
            "enum E1 : short {A, B, C}",
            "enum E2 : byte {A, B, C}");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return (E2)source;");
    }

    [Fact]
    public void CustomClassToEnumWithBaseTypeCastShouldCast()
    {
        var source = TestSourceBuilder.Mapping(
            "C",
            "E",
            "class C { public static explicit operator byte(C c) => 0; } }",
            "enum E : byte {A, B, C}");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return (E)(byte)source;");
    }

    [Fact]
    public void EnumToOtherEnumByValueShouldCast()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByValue)] partial E2 ToE1(E1 source);",
            "enum E1 {A, B, C}",
            "enum E2 {A = 100, B, C}");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return (E2)source;");
    }

    [Fact]
    public void EnumToOtherEnumByNameShouldSwitch()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByName, IgnoreCase = false)] partial E2 ToE1(E1 source);",
            "enum E1 {A, B, C, D, E}",
            "enum E2 {A = 100, B, C, d, e, E}");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"return source switch
    {
        E1.A => E2.A,
        E1.B => E2.B,
        E1.C => E2.C,
        E1.E => E2.E,
        _ => throw new System.ArgumentOutOfRangeException(nameof(source)),
    };".ReplaceLineEndings());
    }

    [Fact]
    public void EnumToOtherEnumByNameIgnoreCaseShouldSwitch()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByName, IgnoreCase = true)] partial E2 ToE1(E1 source);",
            "enum E1 {A, B, C, D, E, f, F}",
            "enum E2 {A = 100, B, C, d, e, E, f}");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"return source switch
    {
        E1.A => E2.A,
        E1.B => E2.B,
        E1.C => E2.C,
        E1.D => E2.d,
        E1.E => E2.E,
        E1.f => E2.f,
        E1.F => E2.f,
        _ => throw new System.ArgumentOutOfRangeException(nameof(source)),
    };".ReplaceLineEndings());
    }

    [Fact]
    public Task EnumToOtherEnumByNameWithoutOverlap()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByName)] partial E2 ToE1(E1 source);",
            "enum E1 {A, B, C}",
            "enum E2 {D, E, F}");

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void EnumToOtherEnumByNameViaGlobalConfigShouldSwitch()
    {
        var source = @"
using System;
using System.Collections.Generic;
using Riok.Mapperly.Abstractions;

[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName)]
public partial class Mapper
{
    partial E2 ToE1(E1 source);
}

enum E1 {A, B, C}

enum E2 {A = 100, B, C}
";

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"return source switch
    {
        E1.A => E2.A,
        E1.B => E2.B,
        E1.C => E2.C,
        _ => throw new System.ArgumentOutOfRangeException(nameof(source)),
    };".ReplaceLineEndings());
    }

    [Fact]
    public void NullableEnumToOtherEnumShouldCastWithNullHandling()
    {
        var source = TestSourceBuilder.Mapping(
            "E1?",
            "E2",
            "enum E1 {A, B, C}",
            "enum E2 {A, B, C}");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return source == null ? throw new System.ArgumentNullException(nameof(source)) : (E2)source.Value;");
    }

    [Fact]
    public void NullableEnumToOtherNullableEnumShouldCast()
    {
        var source = TestSourceBuilder.Mapping(
            "E1?",
            "E2?",
            "enum E1 {A, B, C}",
            "enum E2 {A, B, C}");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return source == null ? default : (E2)source.Value;");
    }

    [Fact]
    public void EnumToOtherNullableEnumShouldCast()
    {
        var source = TestSourceBuilder.Mapping(
            "E1",
            "E2?",
            "enum E1 {A, B, C}",
            "enum E2 {A, B, C}");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return (E2? )(E2)source;");
    }

    [Fact]
    public void EnumToStringShouldSwitch()
    {
        var source = TestSourceBuilder.Mapping(
            "E1",
            "string",
            "enum E1 {A, B, C}");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"return source switch
    {
        E1.A => nameof(E1.A),
        E1.B => nameof(E1.B),
        E1.C => nameof(E1.C),
        _ => source.ToString(),
    };".ReplaceLineEndings());
    }

    [Fact]
    public void StringToEnumShouldSwitchIgnoreCase()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByName, IgnoreCase = true)] partial E1 ToE1(string source);",
            "enum E1 {A, B, C}");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"return source switch
    {
        { } s when s.Equals(nameof(E1.A), System.StringComparison.OrdinalIgnoreCase) => E1.A,
        { } s when s.Equals(nameof(E1.B), System.StringComparison.OrdinalIgnoreCase) => E1.B,
        { } s when s.Equals(nameof(E1.C), System.StringComparison.OrdinalIgnoreCase) => E1.C,
        _ => (E1)Enum.Parse(typeof(E1), source, true),
    };".ReplaceLineEndings());
    }

    [Fact]
    public void StringToEnumShouldSwitch()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByName)] partial E1 ToE1(string source);",
            "enum E1 {A, B, C}");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"return source switch
    {
        nameof(E1.A) => E1.A,
        nameof(E1.B) => E1.B,
        nameof(E1.C) => E1.C,
        _ => (E1)Enum.Parse(typeof(E1), source, false),
    };".ReplaceLineEndings());
    }
}
