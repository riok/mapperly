using System.Runtime.Serialization;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Configuration;

public interface IAttributeDataAccessor
{
    FormatProviderAttribute ReadFormatProviderAttribute(ISymbol symbol);
    MapperConfiguration ReadMapperAttribute(ISymbol symbol);
    MapperIgnoreObsoleteMembersAttribute? ReadMapperIgnoreObsoleteMembersAttribute(ISymbol symbol);
    IEnumerable<NestedMembersMappingConfiguration> ReadMapNestedPropertiesAttribute(ISymbol symbol);
    MapperRequiredMappingAttribute? ReadMapperRequiredMappingAttribute(ISymbol symbol);
    EnumMemberAttribute? ReadEnumMemberAttribute(ISymbol symbol);
    EnumConfiguration? ReadMapEnumAttribute(ISymbol symbol);
    IEnumerable<EnumValueMappingConfiguration> ReadMapEnumValueAttribute(ISymbol symbol);
    IEnumerable<MapperIgnoreEnumValueConfiguration> ReadMapperIgnoreSourceValueAttribute(ISymbol symbol);
    IEnumerable<MapperIgnoreEnumValueConfiguration> ReadMapperIgnoreTargetValueAttribute(ISymbol symbol);
    ComponentModelDescriptionAttributeConfiguration? ReadDescriptionAttribute(ISymbol symbol);
    UserMappingConfiguration? ReadUserMappingAttribute(ISymbol symbol);
    bool HasUseMapperAttribute(ISymbol symbol);
    IEnumerable<MapperIgnoreSourceAttribute> ReadMapperIgnoreSourceAttributes(ISymbol symbol);
    IEnumerable<MapperIgnoreTargetAttribute> ReadMapperIgnoreTargetAttributes(ISymbol symbol);
    IEnumerable<MemberValueMappingConfiguration> ReadMapValueAttribute(ISymbol symbol);
    IEnumerable<MemberMappingConfiguration> ReadMapPropertyAttributes(ISymbol symbol);
    IEnumerable<IncludeMappingConfiguration> ReadIncludeMappingConfigurationAttributes(ISymbol symbol);
    IEnumerable<DerivedTypeMappingConfiguration> ReadMapDerivedTypeAttributes(ISymbol symbol);
    IEnumerable<DerivedTypeMappingConfiguration> ReadGenericMapDerivedTypeAttributes(ISymbol symbol);
    IEnumerable<MemberMappingConfiguration> ReadMapPropertyFromSourceAttributes(ISymbol symbol);
    IEnumerable<UseStaticMapperConfiguration> ReadUseStaticMapperAttributes(ISymbol symbol);
    IEnumerable<UseStaticMapperConfiguration> ReadGenericUseStaticMapperAttributes(ISymbol symbol);
    string GetMappingName(IMethodSymbol methodSymbol);
    bool IsMappingNameEqualTo(IMethodSymbol methodSymbol, string name);
    IEnumerable<NotNullIfNotNullConfiguration> ReadNotNullIfNotNullAttributes(IMethodSymbol symbol);
}
