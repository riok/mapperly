namespace Riok.Mapperly.Abstractions.Test;

public class MapPropertyAttributeTest
{
    [Fact]
    public void ShouldSplitMemberAccess()
    {
        var attr = new MapPropertyAttribute("a.b.c", "d.e.f");
        attr.Source.Should().BeEquivalentTo("a", "b", "c");
        attr.SourceFullName.Should().BeEquivalentTo("a.b.c");
        attr.Target.Should().BeEquivalentTo("d", "e", "f");
        attr.TargetFullName.Should().BeEquivalentTo("d.e.f");
    }
}
