using Riok.Mapperly.Abstractions;
using Riok.Mapperly.IntegrationTests.Models;

namespace Riok.Mapperly.IntegrationTests.Mapper
{
    [Mapper(UseDeepCloning = false)]
    public static partial class ShallowCloningMapper
    {
        [MapperUseShallowCloning]
        public static partial IdObject Copy(IdObject src);

        [MapperIgnoreSource(nameof(TestObject.IgnoredIntValue))]
        [MapperIgnoreSource(nameof(TestObject.IgnoredStringValue))]
        [MapperIgnoreSource(nameof(TestObject.ImmutableHashSetValue))]
        [MapperIgnoreSource(nameof(TestObject.SpanValue))]
        [MapperIgnoreObsoleteMembers]
        [MapperUseShallowCloning]
        public static partial TestObject Copy(TestObject src);
    }
}
