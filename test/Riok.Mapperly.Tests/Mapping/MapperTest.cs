namespace Riok.Mapperly.Tests.Mapping;

[UsesVerify]
public class MapperTest
{
    [Fact]
    public Task SameMapperNameInMultipleNamespacesShouldWork()
    {
        var source = @"
using Riok.Mapperly.Abstractions;

namespace Test.A
{
    [Mapper]
    internal partial class FooBarMapper
    {
        internal partial string FooToBar(string value);
    }
}

namespace Test.B
{
    [Mapper]
    internal partial class FooBarMapper
    {
        internal partial string FooToBar(string value);
    }
}
";

        return TestHelper.VerifyGenerator(source);
    }
}
