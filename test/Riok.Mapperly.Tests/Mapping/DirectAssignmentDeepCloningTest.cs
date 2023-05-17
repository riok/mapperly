namespace Riok.Mapperly.Tests.Mapping;

public class DirectAssignmentDeepCloningTest
{
    [Theory]
    [InlineData("Version")]
    [InlineData("Uri")]
    [InlineData("string")]
    [InlineData("int")]
    public void DirectAssignWithDeepCloning(string type)
    {
        var source = TestSourceBuilder.Mapping(type, type, TestSourceBuilderOptions.WithDeepCloning);
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source;");
    }
}
