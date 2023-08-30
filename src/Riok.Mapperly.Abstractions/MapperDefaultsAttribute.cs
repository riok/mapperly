namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Used to set mapper default values in the assembly.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class MapperDefaultsAttribute : MapperAttribute { }
