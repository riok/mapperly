using System.Linq;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.IntegrationTests.Dto;
using Riok.Mapperly.IntegrationTests.Models;

namespace Riok.Mapperly.IntegrationTests.Mapper
{
    [Mapper(EnumMappingStrategy = EnumMappingStrategy.ByValue, AutoUserMappings = false)]
    public static partial class ProjectionMapper
    {
        public static partial IQueryable<TestObjectDtoProjection> ProjectToDto(this IQueryable<TestObjectProjection> q);

        public static partial IQueryable<TestObjectDtoProjectionBaseType> ProjectToDto(this IQueryable<TestObjectProjectionBaseType> q);

        [MapperIgnoreTarget(nameof(TestObjectDtoProjection.IgnoredStringValue))]
        [MapperIgnoreTarget(nameof(TestObjectDtoProjection.IgnoredIntValue))]
        [MapperIgnoreSource(nameof(TestObjectProjection.IgnoredStringValue))]
        [MapProperty(nameof(TestObjectProjection.RenamedStringValue), nameof(TestObjectDtoProjection.RenamedStringValue2))]
        [MapProperty(
            nameof(TestObjectProjection.ManuallyMappedModified),
            nameof(TestObjectDtoProjection.ManuallyMappedModified),
            Use = nameof(ModifyInt)
        )]
        [MapperIgnoreObsoleteMembers]
        private static partial TestObjectDtoProjection ProjectToDto(this TestObjectProjection testObject);

        [UserMapping]
        private static TestObjectDtoManuallyMappedProjection? MapManual(string str)
        {
            return new TestObjectDtoManuallyMappedProjection(100) { StringValue = str, };
        }

        [UserMapping]
        private static TestEnum MapManual(TestObjectProjectionEnumValue source) => source.Value;

        [MapDerivedType(typeof(TestObjectProjectionTypeA), typeof(TestObjectDtoProjectionTypeA))]
        [MapDerivedType(typeof(TestObjectProjectionTypeB), typeof(TestObjectDtoProjectionTypeB))]
        private static partial TestObjectDtoProjectionBaseType MapDerived(TestObjectProjectionBaseType source);

        private static int ModifyInt(int v) => v + 10;
    }
}
