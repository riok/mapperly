using Microsoft.CodeAnalysis;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.Mappings.Enums;

/// <summary>
/// Represents the mapping configuration for a single enum source parameter in a multi-source enum mapping.
/// </summary>
/// <param name="Parameter">The method parameter representing this enum source</param>
/// <param name="MemberMappings">Dictionary mapping source enum fields to target enum fields</param>
public record EnumSourceMapping(MethodParameter Parameter, IReadOnlyDictionary<IFieldSymbol, IFieldSymbol> MemberMappings);
