using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Symbols.Members;

public interface IMemberGetter
{
    ExpressionSyntax BuildAccess(ExpressionSyntax? baseAccess, bool nullConditional = false);
}
