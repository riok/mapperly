using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Tests.Helpers;

public class ImmutableEquatableArrayTest
{
    [Fact]
    public void SameElementsShouldBeEqual()
    {
        var x1 = Enumerable.Range(1, 3).ToImmutableEquatableArray();
        var x2 = Enumerable.Range(1, 3).ToImmutableEquatableArray();
        x1.GetHashCode().ShouldBe(x2.GetHashCode());
        x1.Equals(x2).ShouldBeTrue();
    }

    [Fact]
    public void DifferentElementsShouldNotBeEqual()
    {
        var x1 = Enumerable.Range(1, 3).ToImmutableEquatableArray();
        var x2 = Enumerable.Range(0, 2).ToImmutableEquatableArray();
        x1.GetHashCode().ShouldNotBe(x2.GetHashCode());
        x1.Equals(x2).ShouldBeFalse();
    }
}
