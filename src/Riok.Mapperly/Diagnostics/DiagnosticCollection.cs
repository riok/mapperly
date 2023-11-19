using System.Collections;
using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Diagnostics;

public class DiagnosticCollection : IReadOnlyCollection<Diagnostic>
{
    private readonly List<Diagnostic> _diagnostics = new();
    private readonly Location _defaultLocation;

    internal DiagnosticCollection(Location defaultLocation)
    {
        _defaultLocation = defaultLocation;
    }

    public IEnumerator<Diagnostic> GetEnumerator() => _diagnostics.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => _diagnostics.Count;

    internal void ReportDiagnostic(DiagnosticDescriptor descriptor, ISymbol? location = null, params object[] messageArgs)
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

        var syntaxNode = location?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
        var nodeLocation = syntaxNode?.GetLocation();
        _diagnostics.Add(Diagnostic.Create(descriptor, nodeLocation ?? _defaultLocation, messageArgs));
    }
}
