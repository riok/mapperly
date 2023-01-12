using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Tests.Helpers;

public class UniqueNameBuilderTest
{
    [Fact]
    public void ShouldGenerateUniqueScopedNames()
    {
        var builder = new UniqueNameBuilder();
        builder.Reserve("FooBar");
        builder.New("FooBar").Should().Be("FooBar1");
        builder.New("FooBar").Should().Be("FooBar2");

        var builder2 = builder.NewScope();
        builder2.Reserve("Baz");
        builder2.New("FooBar").Should().Be("FooBar3");
        builder2.New("Baz").Should().Be("Baz1");
    }
}
