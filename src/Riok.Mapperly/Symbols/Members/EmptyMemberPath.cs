using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Symbols.Members;

[DebuggerDisplay("{RootType} (root)")]
public class EmptyMemberPath(ITypeSymbol rootType) : MemberPath(rootType, [])
{
    public override IMappableMember? Member => null;

    public override ITypeSymbol MemberType => RootType;

    public override string ToDisplayString(bool includeRootType = true, bool includeMemberType = true) =>
        includeRootType ? RootType.ToDisplayString() : string.Empty;
}
