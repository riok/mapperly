using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Symbols;

/// <summary>
/// A mappable member is a member of a class which can take part in a mapping.
/// (eg. a field or a property).
/// </summary>
public interface IMappableMember
{
    string Name { get; }

    ITypeSymbol Type { get; }

    ISymbol MemberSymbol { get; }

    bool IsNullable { get; }

    bool IsIndexer { get; }

    bool CanGet { get; }

    /// <summary>
    /// Whether the member can be modified using assignment or an unsafe accessor method.
    /// </summary>
    bool CanSet { get; }

    /// <summary>
    /// Whether the member can be modified using simple assignment.
    /// </summary>
    bool CanSetDirectly { get; }

    bool IsInitOnly { get; }

    bool IsRequired { get; }

    ExpressionSyntax BuildAccess(ExpressionSyntax source, bool nullConditional = false);
}
