using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Emit;
using Riok.Mapperly.Emit.Syntax;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols.Members;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.UnsafeAccess;

/// <summary>
/// Creates an extension method to access an objects non public property using .Net 8's UnsafeAccessor.
/// <code>
/// [UnsafeAccessor(UnsafeAccessorKind.Property, Name = "get_value")]
/// public extern static int GetValue(this global::MyClass source);
/// </code>
/// </summary>
public class UnsafeGetPropertyAccessor(IPropertySymbol symbol, string methodName) : IUnsafeAccessor, IMemberGetter
{
    private const string DefaultSourceParameterName = "source";

    public MethodDeclarationSyntax BuildAccessorMethod(SourceEmitterContext ctx)
    {
        var nameBuilder = ctx.NameBuilder.NewScope();
        var sourceName = nameBuilder.New(DefaultSourceParameterName);

        var source = Parameter(symbol.ContainingType.FullyQualifiedIdentifierName(), sourceName, true);

        var parameters = ParameterList(CommaSeparatedList(source));
        var attribute = ctx.SyntaxFactory.UnsafeAccessorAttribute(UnsafeAccessorType.Method, $"get_{symbol.Name}");
        return ctx.SyntaxFactory.PublicStaticExternMethod(
            IdentifierName(symbol.Type.FullyQualifiedIdentifierName()).AddTrailingSpace(),
            methodName,
            parameters,
            [attribute]
        );
    }

    public ExpressionSyntax BuildAccess(ExpressionSyntax? baseAccess, bool nullConditional = false)
    {
        if (baseAccess == null)
            throw new ArgumentNullException(nameof(baseAccess));

        ExpressionSyntax method = nullConditional ? ConditionalAccess(baseAccess, methodName) : MemberAccess(baseAccess, methodName);
        return InvocationWithoutIndention(method);
    }
}
