using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Configuration;

/// <summary>
/// Roslyn representation of <see cref="MapDerivedTypeAttribute"/>
/// (use <see cref="ITypeSymbol"/> instead of <see cref="Type"/>).
/// Keep in sync with <see cref="MapDerivedTypeAttribute"/>
/// </summary>
/// <param name="SourceType">The source type of the derived type mapping.</param>
/// <param name="TargetType">The target type of the derived type mapping.</param>
public record DerivedTypeMappingConfiguration(ITypeSymbol SourceType, ITypeSymbol TargetType);
