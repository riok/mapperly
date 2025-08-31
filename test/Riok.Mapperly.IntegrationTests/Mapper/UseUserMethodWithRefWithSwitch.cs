using Riok.Mapperly.Abstractions;
using Riok.Mapperly.IntegrationTests.Models;

namespace Riok.Mapperly.IntegrationTests.Mapper
{
    [Mapper(AllowNullPropertyAssignment = false, UseDeepCloning = true, RequiredMappingStrategy = RequiredMappingStrategy.Source)]
    public static partial class UseUserMethodWithRefWithSwitch
    {
        [MapProperty(
            nameof(TestObjectProjectionBaseType.BaseValue),
            nameof(TestObjectProjectionBaseType.BaseValue),
            Use = nameof(MapIntSum)
        )] // Use is required otherwise it will generate it's own
        [MapDerivedType(typeof(TestObjectProjectionTypeA), typeof(TestObjectProjectionTypeA))]
        [MapDerivedType(typeof(TestObjectProjectionTypeB), typeof(TestObjectProjectionTypeB))]
        public static partial void Merge([MappingTarget] TestObjectProjectionBaseType target, TestObjectProjectionBaseType second);

        private static void MapIntSum([MappingTarget] ref int target, int second) => target = target + second;
    }
}
