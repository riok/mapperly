using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis.CSharp;

namespace Riok.Mapperly.Descriptors;

[StructLayout(LayoutKind.Auto)]
public readonly struct SupportedFeatures
{
    public static SupportedFeatures Build(WellKnownTypes types, SymbolAccessor accessor, LanguageVersion parseLanguageVersion)
    {
        return new()
        {
#if ROSLYN4_4_OR_GREATER
            // nameof(parameter) was introduced in c# 11.0
            NameOfParameter = parseLanguageVersion >= LanguageVersion.CSharp11,
#endif
            NullableAttributes = types.NotNullIfNotNullAttribute != null && accessor.IsDirectlyAccessible(types.NotNullIfNotNullAttribute),
        };
    }

    public bool NameOfParameter { get; private init; }

    public bool NullableAttributes { get; private init; }
}
