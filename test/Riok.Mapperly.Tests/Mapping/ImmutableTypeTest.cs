namespace Riok.Mapperly.Tests.Mapping;

public class ImmutableTypeTest
{
    [Fact]
    public void StringToString()
    {
        var source = TestSourceBuilder.Mapping(
            "string",
            "string");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return source;");
    }

    [Fact]
    public void ReadOnlyStructToSameStruct()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "A",
            "readonly struct A {}");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return source;");
    }
}
