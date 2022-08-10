namespace Riok.Mapperly.Abstractions.Test;

public class MapPropertyAttributeTest
{
    [Fact]
    public void ShouldSplitMemberAccess()
    {
        var attr = new MapPropertyAttribute("a.b.c", "d.e.f");
        attr.Source.Should().BeEquivalentTo(new[] { "a", "b", "c" }, o => o.WithStrictOrdering());
        attr.SourceFullName.Should().Be("a.b.c");
        attr.Target.Should().BeEquivalentTo(new[] { "d", "e", "f" }, o => o.WithStrictOrdering());
        attr.TargetFullName.Should().Be("d.e.f");
    }
}
