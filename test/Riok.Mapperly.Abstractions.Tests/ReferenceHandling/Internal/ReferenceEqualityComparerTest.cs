using Riok.Mapperly.Abstractions.ReferenceHandling.Internal;

namespace Riok.Mapperly.Abstractions.Tests.ReferenceHandling.Internal;

public class ReferenceEqualityComparerTest
{
    [Fact]
    public void PrimitivesShouldNotBeEqual()
    {
        ReferenceEqualityComparer<int>.Instance.Equals(20, 10)
            .Should()
            .BeFalse();
        ReferenceEqualityComparer.Instance.GetHashCode(10)
            .Should()
            .NotBe(ReferenceEqualityComparer.Instance.GetHashCode(10));
    }

    [Fact]
    public void InternedStringsShouldBeEqual()
    {
        ReferenceEqualityComparer<string>.Instance.Equals(string.Intern("fooBar"), string.Intern("fooBar"))
            .Should()
            .BeTrue();
        ReferenceEqualityComparer.Instance.GetHashCode(string.Intern("fooBar"))
            .Should()
            .Be(ReferenceEqualityComparer.Instance.GetHashCode(string.Intern("fooBar")));
    }

    [Fact]
    public void SameObjectRefShouldBeEqual()
    {
        var obj = new object();

        ReferenceEqualityComparer<object>.Instance.Equals(obj, obj)
            .Should()
            .BeTrue();

        ReferenceEqualityComparer.Instance.GetHashCode(obj)
            .Should()
            .Be(ReferenceEqualityComparer.Instance.GetHashCode(obj));
    }

    [Fact]
    public void DifferentObjectRefShouldNotBeEqual()
    {
        ReferenceEqualityComparer<object>.Instance.Equals(new object(), new object())
            .Should()
            .BeFalse();

        ReferenceEqualityComparer.Instance.GetHashCode(new object())
            .Should()
            .NotBe(ReferenceEqualityComparer.Instance.GetHashCode(new object()));
    }
}
