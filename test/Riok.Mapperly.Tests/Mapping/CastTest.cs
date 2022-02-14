namespace Riok.Mapperly.Tests.Mapping;

public class CastTest
{
    [Theory]
    [InlineData("decimal", "float")]
    [InlineData("int", "byte")]
    [InlineData("long", "int")]
    public void NumericExplicitCast(string from, string to)
    {
        var source = TestSourceBuilder.Mapping(from, to);
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be($"return ({to})source;");
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
        var source = TestSourceBuilder.Mapping(from, to);
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be($"return ({to})source;");
    }

    [Fact]
    public void OperatorExplicitClass()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "string",
            "class A { public static explicit operator string(A a) => \"A\"; }");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return (string)source;");
    }

    [Fact]
    public void OperatorExplicitStruct()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "string",
            "struct A { public static explicit operator string(A a) => \"A\"; }");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return (string)source;");
    }

    [Fact]
    public void OperatorReverseExplicitClass()
    {
        var source = TestSourceBuilder.Mapping(
            "string",
            "A",
            "class A { public static explicit operator A(string s) => new(); }");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return (A)source;");
    }

    [Fact]
    public void OperatorReverseExplicitStruct()
    {
        var source = TestSourceBuilder.Mapping(
            "string",
            "A",
            "struct A { public static explicit operator A(string s) => new(); }");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return (A)source;");
    }

    [Fact]
    public void OperatorImplicit()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "string",
            "class A { public static implicit operator string(A a) => \"A\"; }");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return (string)source;");
    }

    [Fact]
    public void OperatorReverseImplicit()
    {
        var source = TestSourceBuilder.Mapping(
            "string",
            "A",
            "class A { public static implicit operator A(string s) => new(); }");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return (A)source;");
    }
}
