namespace Riok.Mapperly.Tests.Mapping;

public class CtorTest
{
    [Fact]
    public void CtorCustomClass()
    {
        var source = TestSourceBuilder.Mapping(
            "string",
            "A",
            "class A { public A(string x) {} }");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return new A(source);");
    }

    [Fact]
    public void CtorCustomStruct()
    {
        var source = TestSourceBuilder.Mapping(
            "string",
            "A",
            "struct A { public A(string x) {} }");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return new A(source);");
    }
}
