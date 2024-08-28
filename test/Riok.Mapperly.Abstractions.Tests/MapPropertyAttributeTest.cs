namespace Riok.Mapperly.Abstractions.Tests;

public class MapPropertyAttributeTest
{
    [Fact]
    public void ShouldSplitMemberAccess()
    {
        var attr = new MapPropertyAttribute("a.b.c", "d.e.f");
        attr.Source.Should().BeEquivalentTo(["a", "b", "c"], o => o.WithStrictOrdering());
        attr.SourceFullName.Should().Be("a.b.c");
        attr.Target.Should().BeEquivalentTo(["d", "e", "f"], o => o.WithStrictOrdering());
        attr.TargetFullName.Should().Be("d.e.f");
    }
}
