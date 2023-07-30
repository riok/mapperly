using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

[UsesVerify]
public class ExtraParametersTest
{
    [Fact]
    public Task MapWithAdditionalParameter()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes("partial string MapTo(int source, string format);");
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task MapShouldNotPassParametersDown()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B MapTo(A source, string format);",
            "class A { public int[] Collection { get; set; } }",
            "class B { public string[] Collection { get; set; } }"
        );
        return TestHelper.VerifyGenerator(source);
    }
}
