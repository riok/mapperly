namespace Riok.Mapperly.Tests.Mapping;

public class SpeicalTypeTest
{
    [Fact]
    public void ClassToObject()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "object",
            "class A {}");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return (object)source;");
    }

    [Fact]
    public void StringToObject()
    {
        var source = TestSourceBuilder.Mapping(
            "string",
            "object");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return (object)source;");
    }

    [Fact]
    public void BuiltInStructToObject()
    {
        var source = TestSourceBuilder.Mapping(
            "DateTime",
            "object");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return (object)source;");
    }

    [Fact]
    public void CustomStructToObject()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "object",
            "struct A {}");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return (object)source;");
    }
}
