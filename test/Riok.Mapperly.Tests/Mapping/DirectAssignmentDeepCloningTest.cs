namespace Riok.Mapperly.Tests.Mapping;

public class DirectAssignmentDeepCloningTest
{
    [Theory]
    [InlineData("Version")]
    [InlineData("Uri")]
    [InlineData("string")]
    [InlineData("int")]
    [InlineData("System.Collections.Immutable.ImmutableHashSet<string>")]
    [InlineData("System.Collections.Immutable.ImmutableDictionary<string>")]
    public void DirectAssignWithDeepCloning(string type)
    {
        var source = TestSourceBuilder.Mapping(type, type, TestSourceBuilderOptions.WithDeepCloning);
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source;");
    }
}
