namespace Riok.Mapperly.Tests.Mapping;

public class ToStringTest
{
    [Fact]
    public void CustomClassToString()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "string",
            "class A {}");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return source.ToString();");
    }

    [Fact]
    public void CustomStructToString()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "string",
            "struct A {}");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return source.ToString();");
    }

    [Fact]
    public void BuiltInStructToString()
    {
        var source = TestSourceBuilder.Mapping(
            "DateTime",
            "string");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return source.ToString();");
    }

    [Fact]
    public void ObjectToString()
    {
        var source = TestSourceBuilder.Mapping(
            "object",
            "string");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return source.ToString();");
    }
}
