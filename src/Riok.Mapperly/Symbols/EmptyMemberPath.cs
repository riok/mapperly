using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Symbols;

[DebuggerDisplay("{RootType} (root)")]
public class EmptyMemberPath(ITypeSymbol rootType) : MemberPath(rootType, Array.Empty<IMappableMember>())
{
    public override IMappableMember? Member => null;

    public override ITypeSymbol MemberType => RootType;

    public override string ToDisplayString(bool includeMemberType = true) => RootType.ToDisplayString();
}
