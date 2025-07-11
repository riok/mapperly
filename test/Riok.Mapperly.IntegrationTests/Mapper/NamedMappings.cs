using Riok.Mapperly.Abstractions;
using Riok.Mapperly.IntegrationTests.Dto;
using Riok.Mapperly.IntegrationTests.Models;

namespace Riok.Mapperly.IntegrationTests.Mapper
{
    [Mapper(AutoUserMappings = false)]
    public static partial class NamedMappings
    {
        [MapValue(nameof(NamedMappingValuesDto.FromMapValue), Use = "CustomStringValueBuilder")]
        [MapProperty(nameof(NamedMappingObject.SourceValue), nameof(NamedMappingValuesDto.FromMapPropertyUse), Use = "CustomModifyValue")]
        [MapPropertyFromSource(nameof(NamedMappingValuesDto.FromMapPropertyFromSource), Use = "CustomUseSource")]
        [NamedMapping("CustomMappingName")]
        public static partial NamedMappingValuesDto MapWithNamedMappings(NamedMappingObject source);

        [IncludeMappingConfiguration("CustomMappingName")]
        public static partial void UpdateDto(NamedMappingObject source, NamedMappingValuesDto target);

        [NamedMapping("CustomStringValueBuilder")]
        private static string StringValueBuilder() => "fooBar";

        [NamedMapping("CustomModifyValue")]
        private static string ModifyValue(string text) => text + "-modified";

        [NamedMapping("CustomUseSource")]
        private static string UseSource(NamedMappingObject source) => source.SourceValue + "-from-source";
    }
}
