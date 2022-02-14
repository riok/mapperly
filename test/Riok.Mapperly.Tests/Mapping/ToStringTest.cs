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
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return source.ToString();");
    }

    [Fact]
    public void CustomStructToString()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "string",
            "struct A {}");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return source.ToString();");
    }

    [Fact]
    public void BuiltInStructToString()
    {
        var source = TestSourceBuilder.Mapping(
            "DateTime",
            "string");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return source.ToString();");
    }

    [Fact]
    public void ObjectToString()
    {
        var source = TestSourceBuilder.Mapping(
            "object",
            "string");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return source.ToString();");
    }
}
