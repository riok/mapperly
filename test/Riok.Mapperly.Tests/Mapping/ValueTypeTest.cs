namespace Riok.Mapperly.Tests.Mapping;

public class ValueTypeTest
{
    [Fact]
    public void CustomReadOnlyStructToSameCustomReadOnlyStruct()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "A",
            "readonly struct A {}");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return source;");
    }

    [Fact]
    public void CustomReadOnlyStructToSameCustomStruct()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "A",
            "readonly struct A {}");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return source;");
    }

    [Fact]
    public void StructToSameStruct()
    {
        var source = TestSourceBuilder.Mapping(
            "DateTime",
            "DateTime");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return source;");
    }
}
