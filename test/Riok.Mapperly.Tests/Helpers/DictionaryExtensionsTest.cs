using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Tests.Helpers;

public class DictionaryExtensionsTest
{
    [Fact]
    public void RemoveRangeShouldRemoveEntries()
    {
        var d = new Dictionary<string, int>
        {
            ["a"] = 10,
            ["b"] = 20,
            ["c"] = 30,
        };
        d.RemoveRange(["a", "c"]);
        d.Keys.ShouldBe(["b"]);
    }
}
