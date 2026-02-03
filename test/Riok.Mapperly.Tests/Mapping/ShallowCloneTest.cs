namespace Riok.Mapperly.Tests.Mapping;

public class ShallowCloneTest
{
    [Fact]
    public void ShallowCloneShouldNotReturnOriginalInstance()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "A",
            TestSourceBuilderOptions.WithShallowCloning,
            "class A { public int Value { get; set; } public List<string> List { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::A();
                target.Value = source.Value;
                target.List = source.List;
                return target;
                """
            );
    }
}
