using System.Diagnostics;

namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Marks a readable member on a mapper which provides an <see cref="IServiceProvider"/> instance.
/// When present, nested complex member mappings can optionally resolve an <c>IMapper&lt;TS, TD&gt;</c> service
/// from the provider and delegate the nested mapping to it; otherwise the generated mapping is used.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
[Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
public sealed class MapperServiceProviderAttribute : Attribute;
