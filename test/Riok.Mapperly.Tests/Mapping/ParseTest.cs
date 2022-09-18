namespace Riok.Mapperly.Tests.Mapping;

public class ParseTest
{
    [Fact]
    public void ParseableBuiltInStruct()
    {
        var source = TestSourceBuilder.Mapping("string", "DateTime");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return System.DateTime.Parse(source);");
    }

    [Fact]
    public void ParseableBuiltInClass()
    {
        var source = TestSourceBuilder.Mapping("string", "Version");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return System.Version.Parse(source);");
    }

    [Fact]
    public void ParseableBuiltNullableInClass()
    {
        var source = TestSourceBuilder.Mapping("string?", "int?");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return source == null ? default : int.Parse(source);");
    }

    [Fact]
    public void ParseableCustomStruct()
    {
        var source = TestSourceBuilder.Mapping(
            "string",
            "A",
            "struct A { public static A Parse(string v) => new(); }");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return A.Parse(source);");
    }

    [Fact]
    public void ParseableCustomClass()
    {
        var source = TestSourceBuilder.Mapping(
            "string",
            "A",
            "class A { public static A Parse(string v) => new(); }");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return A.Parse(source);");
    }
}
