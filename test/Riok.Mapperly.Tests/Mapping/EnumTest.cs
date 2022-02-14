namespace Riok.Mapperly.Tests.Mapping;

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
    public void EnumToOtherEnumByValueShouldSwitch()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByValue)] E2 ToE1(E1 source);",
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
            "[MapEnum(EnumMappingStrategy.ByName)] E2 ToE1(E1 source);",
            "enum E1 {A, B, C}",
            "enum E2 {A = 100, B, C}");

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
    public void EnumToOtherEnumByNameViaGlobalConfigShouldSwitch()
    {
        var source = @"
using System;
using System.Collections.Generic;
using Riok.Mapperly.Abstractions;

[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName)]
public interface IMapper
{
    E2 ToE1(E1 source);
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
            .Be("return source == null ? default : (E2)source.Value;");
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
            .Be("return (E2)source;");
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
    public void StringToEnumShouldSwitch()
    {
        var source = TestSourceBuilder.Mapping(
            "string",
            "E1",
            "enum E1 {A, B, C}");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"return source switch
    {
        nameof(E1.A) => E1.A,
        nameof(E1.B) => E1.B,
        nameof(E1.C) => E1.C,
        _ => (E1)Enum.Parse(typeof(E1), source),
    };".ReplaceLineEndings());
    }
}
