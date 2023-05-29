using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Configuration;

/// <summary>
/// Roslyn representation of <see cref="MapEnumValueAttribute"/>
/// Keep in sync with <see cref="MapEnumValueAttribute"/>
/// </summary>
/// <param name="Source">The source constant of the enum value mapping.</param>
/// <param name="Target">The target constant of the enum value mapping.</param>
public record MapEnumValue(TypedConstant Source, TypedConstant Target);
