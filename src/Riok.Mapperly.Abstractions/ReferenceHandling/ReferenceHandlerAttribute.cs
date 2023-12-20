using System.Diagnostics;

namespace Riok.Mapperly.Abstractions.ReferenceHandling;

/// <summary>
/// Marks a mapping method parameter as a <see cref="IReferenceHandler"/>.
/// The type of the parameter needs to be <see cref="IReferenceHandler"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
[Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
public sealed class ReferenceHandlerAttribute : Attribute;
