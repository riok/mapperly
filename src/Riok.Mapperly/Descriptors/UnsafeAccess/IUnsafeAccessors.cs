using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Emit;

namespace Riok.Mapperly.Descriptors.UnsafeAccess;

public interface IUnsafeAccessors
{
    int Count { get; }

    IEnumerable<MemberDeclarationSyntax> Build(SourceEmitterContext ctx, CancellationToken cancellationToken);
}
