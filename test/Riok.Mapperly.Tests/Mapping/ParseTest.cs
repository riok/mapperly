namespace Riok.Mapperly.Tests.Mapping;

public class ParseTest
{
    [Fact]
    public void ParseableBuiltInStruct()
    {
        var source = TestSourceBuilder.Mapping("string", "DateTime");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return System.DateTime.Parse(source);");
    }

    [Fact]
    public void ParseableBuiltInClass()
    {
        var source = TestSourceBuilder.Mapping("string", "Version");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return System.Version.Parse(source);");
    }

    [Fact]
    public void ParseableBuiltNullableInClass()
    {
        var source = TestSourceBuilder.Mapping("string?", "int?");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return int.Parse(source);");
    }

    [Fact]
    public void ParseableCustomStruct()
    {
        var source = TestSourceBuilder.Mapping(
            "string",
            "A",
            "struct A { public static A Parse(string v) => new(); }");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return A.Parse(source);");
    }

    [Fact]
    public void ParseableCustomClass()
    {
        var source = TestSourceBuilder.Mapping(
            "string",
            "A",
            "class A { public static A Parse(string v) => new(); }");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return A.Parse(source);");
    }
}
