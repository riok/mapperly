using PublicApiGenerator;

namespace Riok.Mapperly.Abstractions.Tests;

public class PublicApiTest
{
    [Fact]
    public Task PublicApiHasNotChanged()
    {
        var assembly = typeof(MapperAttribute).Assembly;
        var api = assembly.GeneratePublicApi();
        return Verify(api, "cs");
    }
}
