using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Emit;
using Riok.Mapperly.Emit.Syntax;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols.Members;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.UnsafeAccess;

/// <summary>
/// Creates an extension method to access an objects non public field using .Net 8's UnsafeAccessor.
/// <code>
/// [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_value")]
/// public extern static int GetValue(this global::MyClass source);
/// </code>
/// </summary>
public class UnsafeFieldAccessor(IFieldSymbol symbol, string methodName) : IUnsafeAccessor, IMemberSetter, IMemberGetter
{
    private const string DefaultTargetParameterName = "target";

    public bool SupportsCoalesceAssignment => true;

    public MethodDeclarationSyntax BuildAccessorMethod(SourceEmitterContext ctx)
    {
        var nameBuilder = ctx.NameBuilder.NewScope();
        var targetName = nameBuilder.New(DefaultTargetParameterName);

        var target = Parameter(symbol.ContainingType.FullyQualifiedIdentifierName(), targetName, true);

        var parameters = ParameterList(CommaSeparatedList(target));
        var attributeList = ctx.SyntaxFactory.UnsafeAccessorAttributeList(UnsafeAccessorType.Field, symbol.Name);
        var returnType = RefType(IdentifierName(symbol.Type.FullyQualifiedIdentifierName()).AddTrailingSpace())
            .WithRefKeyword(Token(TriviaList(), SyntaxKind.RefKeyword, TriviaList(Space)));

        return PublicStaticExternMethod(ctx, returnType, methodName, parameters, attributeList);
    }

    public ExpressionSyntax BuildAccess(ExpressionSyntax? baseAccess, bool nullConditional = false)
    {
        if (baseAccess == null)
            throw new ArgumentNullException(nameof(baseAccess));

        ExpressionSyntax method = nullConditional ? ConditionalAccess(baseAccess, methodName) : MemberAccess(baseAccess, methodName);
        return Invocation(method);
    }

    public ExpressionSyntax BuildAssignment(ExpressionSyntax? baseAccess, ExpressionSyntax valueToAssign, bool coalesceAssignment = false)
    {
        var access = BuildAccess(baseAccess);
        return Assignment(access, valueToAssign, coalesceAssignment);
    }
}
