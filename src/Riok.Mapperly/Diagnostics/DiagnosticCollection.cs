using System.Collections;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Diagnostics;

public class DiagnosticCollection(Location defaultLocation) : IReadOnlyCollection<Diagnostic>
{
    private readonly List<Diagnostic> _diagnostics = [];

    public IEnumerator<Diagnostic> GetEnumerator() => _diagnostics.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => _diagnostics.Count;

    internal void ReportDiagnostic(DiagnosticDescriptor descriptor, ISymbol? symbolLocation, params object?[] messageArgs) =>
        ReportDiagnostic(descriptor, symbolLocation?.GetSyntaxLocation(), messageArgs);

    internal void ReportDiagnostic(DiagnosticDescriptor descriptor, Location? location = null, params object?[] messageArgs)
    {
        // cannot use the symbol since it would break the incremental generator
        // due to being different for each compilation.
        for (var i = 0; i < messageArgs.Length; i++)
        {
            if (messageArgs[i] is ISymbol symbol)
            {
                messageArgs[i] = symbol.ToDisplayString();
            }
        }

        var diagnostic = Diagnostic.Create(descriptor, location ?? defaultLocation, messageArgs);

        if (
            _diagnostics.Exists(d =>
                ReferenceEquals(d.Descriptor, diagnostic.Descriptor)
                && Equals(d.Location, diagnostic.Location)
                && Equals(d.Severity, diagnostic.Severity)
                && Equals(d.WarningLevel, diagnostic.WarningLevel)
                && Equals(d.GetMessage(), diagnostic.GetMessage())
            )
        )
        {
            return;
        }

        _diagnostics.Add(diagnostic);
    }
}
