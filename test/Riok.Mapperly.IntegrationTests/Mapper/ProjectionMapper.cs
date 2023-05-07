using System.Linq;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.IntegrationTests.Dto;
using Riok.Mapperly.IntegrationTests.Models;

namespace Riok.Mapperly.IntegrationTests.Mapper
{
    [Mapper]
    public static partial class ProjectionMapper
    {
        public static partial IQueryable<TestObjectDtoProjection> ProjectToDto(this IQueryable<TestObjectProjection> q);

        public static partial IQueryable<TestObjectDtoProjectionBaseType> ProjectToDto(this IQueryable<TestObjectProjectionBaseType> q);

        // disable obsolete warning, as the obsolete attribute should still be tested.
#pragma warning disable CS0618
        [MapperIgnore(nameof(TestObjectDtoProjection.IgnoredStringValue))]
#pragma warning restore CS0618
        [MapperIgnoreTarget(nameof(TestObjectDtoProjection.IgnoredIntValue))]
        [MapperIgnoreSource(nameof(TestObjectProjection.IgnoredStringValue))]
        [MapProperty(nameof(TestObjectProjection.RenamedStringValue), nameof(TestObjectDtoProjection.RenamedStringValue2))]
        private static partial TestObjectDtoProjection ProjectToDto(this TestObjectProjection testObject);

        private static TestObjectDtoManuallyMappedProjection? MapManual(string str)
        {
            return new TestObjectDtoManuallyMappedProjection(100) { StringValue = str, };
        }

        [MapDerivedType(typeof(TestObjectProjectionTypeA), typeof(TestObjectDtoProjectionTypeA))]
        [MapDerivedType(typeof(TestObjectProjectionTypeB), typeof(TestObjectDtoProjectionTypeB))]
        private static partial TestObjectDtoProjectionBaseType MapDerived(TestObjectProjectionBaseType source);
    }
}
