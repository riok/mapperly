using Riok.Mapperly.Abstractions;
using Riok.Mapperly.IntegrationTests.Dto;
using Riok.Mapperly.IntegrationTests.Models;

namespace Riok.Mapperly.IntegrationTests.Mapper
{
    [Mapper(AutoUserMappings = false)]
    public static partial class NamedMappings
    {
        [MapValue(nameof(NamedMappedValuesDto.FromMapValue), Use = "CustomStringValueBuilder")]
        [MapProperty(nameof(NamedMappingObject.SourceValue), nameof(NamedMappedValuesDto.FromMapPropertyUse), Use = "CustomModifyValue")]
        [MapPropertyFromSource(nameof(NamedMappedValuesDto.FromMapPropertyFromSource), Use = "CustomUseSource")]
        [NamedMapping("CustomMappingName")]
        public static partial NamedMappedValuesDto MapWithNamedMappings(NamedMappingObject source);

        [IncludeMappingConfiguration("CustomMappingName")]
        public static partial void UpdateDto(NamedMappingObject source, NamedMappedValuesDto target);

        [NamedMapping("CustomStringValueBuilder")]
        private static string StringValueBuilder() => "fooBar";

        [NamedMapping("CustomModifyValue")]
        private static string ModifyValue(string text) => text + "-modified";

        [NamedMapping("CustomUseSource")]
        private static string UseSource(NamedMappingObject source) => source.SourceValue + "-from-source";
    }
}
