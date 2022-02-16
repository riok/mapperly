namespace Riok.Mapperly.Tests.Mapping;

public class SpecialTypeTest
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
    public void ClassToObjectDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "object",
            TestSourceBuilderOptions.WithDeepCloning,
            "class A {}");
        TestHelper.GenerateMapperMethodBody(source)
            .Should()
            .Be("return (object)MapToA(source);");
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
    public void StringToObjectDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "string",
            "object",
            TestSourceBuilderOptions.WithDeepCloning);
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
    public void BuiltInStructToObjectDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "DateTime",
            "object",
            TestSourceBuilderOptions.WithDeepCloning);
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return (object)source;");
    }

    [Fact]
    public void CustomReadOnlyStructToObject()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "object",
            "readonly struct A {}");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return (object)source;");
    }

    [Fact]
    public void CustomReadOnlyStructToObjectDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "object",
            TestSourceBuilderOptions.WithDeepCloning,
            "readonly struct A {}");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return (object)source;");
    }

    [Fact]
    public void CustomMutableStructToObject()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "object",
            "struct A {}");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return (object)source;");
    }

    [Fact]
    public void CustomMutableStructToObjectDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "object",
            TestSourceBuilderOptions.WithDeepCloning,
            "struct A {}");
        TestHelper.GenerateMapperMethodBody(source)
            .Should()
            .Be("return (object)MapToA(source);");
    }
}
