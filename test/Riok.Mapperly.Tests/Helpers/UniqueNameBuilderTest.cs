using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Tests.Helpers;

public class UniqueNameBuilderTest
{
    [Fact]
    public void ShouldGenerateUniqueScopedNames()
    {
        var builder = new UniqueNameBuilder();
        builder.Reserve("FooBar");
        builder.New("FooBar").ShouldBe("FooBar1");
        builder.New("FooBar").ShouldBe("FooBar2");

        var builder2 = builder.NewScope();
        builder2.Reserve("Baz");
        builder2.New("FooBar").ShouldBe("FooBar3");
        builder2.New("Baz").ShouldBe("Baz1");

        builder2.NewIfNeeded("NewName", out var n1).ShouldBeFalse();
        n1.ShouldBe("NewName");

        builder2.NewIfNeeded("FooBar", out var n2).ShouldBeTrue();
        n2.ShouldBe("FooBar4");
    }
}
