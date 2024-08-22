using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Emit.Syntax;

public partial struct SyntaxFactoryHelper
{
    private const string MethodImplAttributeName = "global::System.Runtime.CompilerServices.MethodImpl";

    public SyntaxList<AttributeListSyntax> MethodImplAttributeList()
    {
        return AttributeList(MethodImplAttributeName, EnumLiteral(MethodImplOptions.AggressiveInlining));
    }
}
