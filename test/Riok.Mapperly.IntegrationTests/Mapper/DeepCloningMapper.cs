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
        [MapperIgnoreSource(nameof(TestObject.SpanValue))]
        [MapperIgnoreObsoleteMembers]
        public static partial TestObject Copy(TestObject src);
    }

    [Mapper(CloningStrategy = CloningStrategy.DeepCloning)]
    public static partial class DeepCloningMapperWithCloningStrategy
    {
        public static partial IdObject Copy(IdObject src);

        [MapperIgnoreSource(nameof(TestObject.IgnoredIntValue))]
        [MapperIgnoreSource(nameof(TestObject.IgnoredStringValue))]
        [MapperIgnoreSource(nameof(TestObject.ImmutableHashSetValue))]
        [MapperIgnoreSource(nameof(TestObject.SpanValue))]
        [MapperIgnoreObsoleteMembers]
        public static partial TestObject Copy(TestObject src);
    }
}
