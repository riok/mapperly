using System.Diagnostics;

namespace Riok.Mapperly.Abstractions;

/// <summary>
/// A mapping method marked with this attribute will avoid reusing the same source instance,
/// either by directly returning it or by implicit casting, and will always result in a new instance being returned.
/// This attribute will only apply to mapping methods which have the same source and target types.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
[Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
public sealed class MapperUseShallowCloningAttribute : Attribute { }
