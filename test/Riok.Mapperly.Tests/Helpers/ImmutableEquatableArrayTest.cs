using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Tests.Helpers;

public class ImmutableEquatableArrayTest
{
    [Fact]
    public void SameElementsShouldBeEqual()
    {
        var x1 = Enumerable.Range(1, 3).ToImmutableEquatableArray();
        var x2 = Enumerable.Range(1, 3).ToImmutableEquatableArray();
        x1.GetHashCode().Should().Be(x2.GetHashCode());
        x1.Equals(x2).Should().BeTrue();
    }

    [Fact]
    public void DifferentElementsShouldNotBeEqual()
    {
        var x1 = Enumerable.Range(1, 3).ToImmutableEquatableArray();
        var x2 = Enumerable.Range(0, 2).ToImmutableEquatableArray();
        x1.GetHashCode().Should().NotBe(x2.GetHashCode());
        x1.Equals(x2).Should().BeFalse();
    }
}
