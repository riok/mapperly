using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Riok.Mapperly.Helpers;

public static class ThrowHelpers
{
    [StackTraceHidden]
    public static void ThrowIfNull(
        [NotNull] object? argument,
        [CallerArgumentExpression(nameof(argument))] string? paramName = null,
        [CallerMemberName] string? callerName = null
    )
    {
        if (argument is null)
        {
            throw new ArgumentNullException(paramName, $"{callerName} requires a non-null value for {paramName}.");
        }
    }
}
