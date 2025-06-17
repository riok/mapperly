namespace Riok.Mapperly.Abstractions.Tests;

public class MapPropertyAttributeTest
{
    [Fact]
    public void ShouldSplitMemberAccess()
    {
        var attr = new MapPropertyAttribute("a.b.c", "d.e.f");
        attr.Source.ShouldBe(["a", "b", "c"]);
        attr.SourceFullName.ShouldBe("a.b.c");
        attr.Target.ShouldBe(["d", "e", "f"]);
        attr.TargetFullName.ShouldBe("d.e.f");
    }
}
