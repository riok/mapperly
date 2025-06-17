using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Tests.Helpers;

public class HashSetExtensionsTest
{
    [Fact]
    public void AddRangeShouldAddAllItems()
    {
        var h = new HashSet<int> { 1, 2, 3 };
        h.AddRange([3, 4, 5, 5]);
        h.ShouldContain(1);
        h.ShouldContain(2);
        h.ShouldContain(3);
        h.ShouldContain(4);
        h.ShouldContain(5);
    }
}
