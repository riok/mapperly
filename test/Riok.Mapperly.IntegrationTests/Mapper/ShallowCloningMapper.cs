using Riok.Mapperly.Abstractions;
using Riok.Mapperly.IntegrationTests.Models;

namespace Riok.Mapperly.IntegrationTests.Mapper
{
    [Mapper(CloningStrategy = CloningStrategy.ShallowCloning)]
    public static partial class ShallowCloningMapper
    {
        [UserMapping(Default = false)]
        public static partial IdObject Copy(IdObject src);

        [MapperIgnoreSource(nameof(TestObject.IgnoredIntValue))]
        [MapperIgnoreSource(nameof(TestObject.IgnoredStringValue))]
        [MapperIgnoreSource(nameof(TestObject.ImmutableHashSetValue))]
        [MapperIgnoreSource(nameof(TestObject.SpanValue))]
        [MapperIgnoreObsoleteMembers]
        [UserMapping(Default = false)]
        public static partial TestObject Copy(TestObject src);
    }
}
