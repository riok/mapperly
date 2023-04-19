namespace Riok.Mapperly.Tests.Mapping;

public class ObjectFieldTest
{
    [Fact]
    public void OneSimpleField()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string StringValue; }",
            "class B { public string StringValue; }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.StringValue = source.StringValue;
                return target;
                """
            );
    }

    [Fact]
    public void StringToIntField()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string Value { get; set; } }",
            "class B { public int Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = int.Parse(source.Value);
                return target;
                """
            );
    }
}
