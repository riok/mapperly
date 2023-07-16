using Riok.Mapperly.Abstractions;
using Riok.Mapperly.IntegrationTests.Models;

namespace Riok.Mapperly.IntegrationTests.Mapper
{
    [Mapper(UseDeepCloning = true)]
    public static partial class DeepCloningMapper
    {
        public static partial IdObject Copy(IdObject src);

        [MapperIgnoreSource(nameof(TestObject.IgnoredIntValue))]
        [MapperIgnoreSource(nameof(TestObject.IgnoredStringValue))]
        [MapperIgnoreSource(nameof(TestObject.ImmutableHashSetValue))]
        [MapperIgnoreObsoleteMembers]
        public static partial TestObject Copy(TestObject src);
    }
}
