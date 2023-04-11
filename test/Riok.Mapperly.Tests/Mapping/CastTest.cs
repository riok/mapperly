using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class CastTest
{
    [Theory]
    [InlineData("decimal", "float")]
    [InlineData("int", "byte")]
    [InlineData("long", "int")]
    public void NumericExplicitCast(string from, string to)
    {
        var source = TestSourceBuilder.Mapping(from, to, TestSourceBuilderOptions.WithDeepCloning);
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody($"return ({to})source;");
    }

    [Theory]
    [InlineData("sbyte", "int")]
    [InlineData("byte", "int")]
    [InlineData("ushort", "int")]
    [InlineData("short", "int")]
    [InlineData("uint", "long")]
    [InlineData("int", "long")]
    [InlineData("ulong", "float")]
    [InlineData("long", "float")]
    [InlineData("float", "double")]
    [InlineData("char", "int")]
    public void NumericImplicitCast(string from, string to)
    {
        var source = TestSourceBuilder.Mapping(from, to, TestSourceBuilderOptions.WithDeepCloning);
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody($"return ({to})source;");
    }

    [Fact]
    public void OperatorExplicitClassWithImmutableTarget()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "string",
            "class A { public static explicit operator string(A a) => \"A\"; }");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return (string)source;");
    }

    [Fact]
    public void OperatorExplicitClassWithImmutableTargetDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "string",
            TestSourceBuilderOptions.WithDeepCloning,
            "class A { public static explicit operator string(A a) => \"A\"; }");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return (string)source;");
    }

    [Fact]
    public void OperatorExplicitStructWithImmutableTarget()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "string",
            "struct A { public static explicit operator string(A a) => \"A\"; }");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return (string)source;");
    }

    [Fact]
    public void OperatorExplicitStructWithImmutableTargetDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "string",
            TestSourceBuilderOptions.WithDeepCloning,
            "struct A { public static explicit operator string(A a) => \"A\"; }");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return (string)source;");
    }

    [Fact]
    public void OperatorExplicitClassWithImmutableSource()
    {
        var source = TestSourceBuilder.Mapping(
            "string",
            "A",
            "class A { public static explicit operator A(string s) => new(); }");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return (global::A)source;");
    }

    [Fact]
    public void OperatorExplicitClassWithImmutableSourceDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "string",
            "A",
            TestSourceBuilderOptions.WithDeepCloning,
            "class A { public static explicit operator A(string s) => new(); }");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return (global::A)source;");
    }

    [Fact]
    public void OperatorExplicitStructWithImmutableSource()
    {
        var source = TestSourceBuilder.Mapping(
            "string",
            "A",
            "struct A { public static explicit operator A(string s) => new(); }");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return (global::A)source;");
    }

    [Fact]
    public void OperatorExplicitStructWithImmutableSourceDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "string",
            "A",
            TestSourceBuilderOptions.WithDeepCloning,
            "struct A { public static explicit operator A(string s) => new(); }");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return (global::A)source;");
    }

    [Fact]
    public void OperatorExplicitClassWithClassTarget()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public static explicit operator B(A a) => new(); }",
            "class B {}");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return (global::B)source;");
    }

    [Fact]
    public void OperatorExplicitClassWithClassTargetDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            TestSourceBuilderOptions.WithDeepCloning,
            "class A { public static explicit operator B(A a) => new(); }",
            "class B {}");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                return target;
                """);
    }

    [Fact]
    public void OperatorExplicitStructWithMutableStructTarget()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "struct A { public static explicit operator B(A a) => new(); }",
            "struct B {}");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return (global::B)source;");
    }

    [Fact]
    public void OperatorExplicitStructWithMutableStructTargetDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            TestSourceBuilderOptions.WithDeepCloning,
            "struct A { public static explicit operator B(A a) => new(); }",
            "struct B {}");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                return target;
                """);
    }

    [Fact]
    public void OperatorImplicitClassWithImmutableTarget()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "string",
            "class A { public static implicit operator string(A a) => \"A\"; }");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return (string)source;");
    }

    [Fact]
    public void OperatorImplicitClassWithImmutableTargetDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "string",
            TestSourceBuilderOptions.WithDeepCloning,
            "class A { public static implicit operator string(A a) => \"A\"; }");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return (string)source;");
    }

    [Fact]
    public void OperatorImplicitStructWithImmutableTarget()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "string",
            "struct A { public static implicit operator string(A a) => \"A\"; }");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return (string)source;");
    }

    [Fact]
    public void OperatorImplicitStructWithImmutableTargetDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "string",
            TestSourceBuilderOptions.WithDeepCloning,
            "struct A { public static implicit operator string(A a) => \"A\"; }");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return (string)source;");
    }

    [Fact]
    public void OperatorImplicitClassWithImmutableSource()
    {
        var source = TestSourceBuilder.Mapping(
            "string",
            "A",
            "class A { public static implicit operator A(string s) => new(); }");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return (global::A)source;");
    }

    [Fact]
    public void OperatorImplicitClassWithImmutableSourceDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "string",
            "A",
            TestSourceBuilderOptions.WithDeepCloning,
            "class A { public static implicit operator A(string s) => new(); }");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return (global::A)source;");
    }

    [Fact]
    public void OperatorImplicitStructWithImmutableSource()
    {
        var source = TestSourceBuilder.Mapping(
            "string",
            "A",
            "struct A { public static implicit operator A(string s) => new(); }");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return (global::A)source;");
    }

    [Fact]
    public void OperatorImplicitStructWithImmutableSourceDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "string",
            "A",
            TestSourceBuilderOptions.WithDeepCloning,
            "struct A { public static implicit operator A(string s) => new(); }");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return (global::A)source;");
    }

    [Fact]
    public void OperatorImplicitClassWithClassTarget()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public static implicit operator B(A a) => new(); }",
            "class B {}");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return (global::B)source;");
    }

    [Fact]
    public void OperatorImplicitClassWithClassTargetDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            TestSourceBuilderOptions.WithDeepCloning,
            "class A { public static implicit operator B(A a) => new(); }",
            "class B {}");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                return target;
                """);
    }

    [Fact]
    public void OperatorImplicitStructWithMutableStructTarget()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "struct A { public static implicit operator B(A a) => new(); }",
            "struct B {}");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return (global::B)source;");
    }

    [Fact]
    public void OperatorImplicitStructWithMutableStructTargetDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            TestSourceBuilderOptions.WithDeepCloning,
            "struct A { public static implicit operator B(A a) => new(); }",
            "struct B {}");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                return target;
                """);
    }

    [Fact]
    public void ImplicitCastMappingDisabledShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "byte",
            "int",
            TestSourceBuilderOptions.WithDisabledMappingConversion(MappingConversionType.ImplicitCast));
        TestHelper.GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(new(DiagnosticDescriptors.CouldNotCreateMapping));
    }

    [Fact]
    public void ExplicitCastMappingDisabledShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "int",
            "byte",
            TestSourceBuilderOptions.WithDisabledMappingConversion(MappingConversionType.ExplicitCast));
        TestHelper.GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(new(DiagnosticDescriptors.CouldNotCreateMapping));
    }
}
