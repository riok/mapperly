namespace Riok.Mapperly.Tests.Mapping;

[UsesVerify]
public class DateTimeTest
{
    [Fact]
    public void DateTimeToDateOnly()
    {
        var source = TestSourceBuilder.Mapping(
            "DateTime",
            "DateOnly");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return global::System.DateOnly.FromDateTime(source);");
    }

    [Fact]
    public void DateTimeToTimeOnly()
    {
        var source = TestSourceBuilder.Mapping(
            "DateTime",
            "TimeOnly");
        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody("return global::System.TimeOnly.FromDateTime(source);");
    }
}
