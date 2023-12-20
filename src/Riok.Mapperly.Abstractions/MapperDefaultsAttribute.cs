using System.Diagnostics;

namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Used to set mapper default values in the assembly.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
[Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
public sealed class MapperDefaultsAttribute : MapperAttribute;
