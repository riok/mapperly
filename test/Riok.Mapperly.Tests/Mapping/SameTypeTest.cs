namespace Riok.Mapperly.Tests.Mapping;

public class SameTypeTest
{
    [Fact]
    public void ReadOnlyStructToSameReadOnlyStruct()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "A",
            "readonly struct A {}");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return source;");
    }

    [Fact]
    public void ReadOnlyStructToSameReadOnlyStructDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "A",
            TestSourceBuilderOptions.WithDeepCloning,
            "readonly struct A {}");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return source;");
    }

    [Fact]
    public void StringToString()
    {
        var source = TestSourceBuilder.Mapping(
            "string",
            "string");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return source;");
    }

    [Fact]
    public void StringToStringDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "string",
            "string",
            TestSourceBuilderOptions.WithDeepCloning);
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return source;");
    }

    [Fact]
    public void MutableStructToSameMutableStruct()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "A",
            "struct A {}");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return source;");
    }

    [Fact]
    public void MutableStructToSameMutableStructDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "A",
            TestSourceBuilderOptions.WithDeepCloning,
            "struct A {}");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(@"var target = new A();
    return target;");
    }

    [Fact]
    public void ClassToSameClass()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "A",
            "class A { public string StringValue { get; set; } }");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return source;");
    }

    [Fact]
    public void ClassToSameClassDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "A",
            TestSourceBuilderOptions.WithDeepCloning,
            "class A {}");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(@"var target = new A();
    return target;");
    }
}
