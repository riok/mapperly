using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Tests.Helpers;

public class HashSetExtensionsTest
{
    [Fact]
    public void AddRangeShouldAddAllItems()
    {
        var h = new HashSet<int> { 1, 2, 3 };
        h.AddRange([3, 4, 5, 5]);
        h.Should().Contain([1, 2, 3, 4, 5]);
    }
}
