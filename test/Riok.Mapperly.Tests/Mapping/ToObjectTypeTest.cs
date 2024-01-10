namespace Riok.Mapperly.Tests.Mapping;

public class ToObjectTypeTest
{
    [Fact]
    public void ClassToObject()
    {
        var source = TestSourceBuilder.Mapping("A", "object", "class A {}");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return (object)source;");
    }

    [Fact]
    public void StringToObject()
    {
        var source = TestSourceBuilder.Mapping("string", "object");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return (object)source;");
    }

    [Fact]
    public void BuiltInStructToObject()
    {
        var source = TestSourceBuilder.Mapping("DateTime", "object");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return (object)source;");
    }

    [Fact]
    public void CustomReadOnlyStructToObject()
    {
        var source = TestSourceBuilder.Mapping("A", "object", "readonly struct A {}");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return (object)source;");
    }

    [Fact]
    public void CustomMutableStructToObject()
    {
        var source = TestSourceBuilder.Mapping("A", "object", "struct A {}");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return (object)source;");
    }
}
