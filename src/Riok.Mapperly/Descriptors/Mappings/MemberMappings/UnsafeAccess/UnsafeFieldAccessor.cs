using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Emit;
using Riok.Mapperly.Emit.Syntax;
using Riok.Mapperly.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.MemberMappings.UnsafeAccess;

/// <summary>
/// Creates an extension method to access an objects non public field using .Net 8's UnsafeAccessor.
/// /// <code>
/// [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_value")]
/// public extern static int GetValue(this global::MyClass source);
/// </code>
/// </summary>
public class UnsafeFieldAccessor : IUnsafeAccessor
{
    private const string DefaultTargetParameterName = "target";

    private readonly string _targetType;
    private readonly string _result;
    private readonly string _memberName;

    public UnsafeFieldAccessor(IFieldSymbol value, string methodName)
    {
        MethodName = methodName;
        _targetType = value.ContainingType.FullyQualifiedIdentifierName();
        _result = value.Type.FullyQualifiedIdentifierName();
        _memberName = value.Name;
    }

    public string MethodName { get; }

    public MethodDeclarationSyntax BuildMethod(SourceEmitterContext ctx)
    {
        var nameBuilder = ctx.NameBuilder.NewScope();
        var targetName = nameBuilder.New(DefaultTargetParameterName);

        var target = Parameter(_targetType, targetName, true);

        var parameters = ParameterList(CommaSeparatedList(target));
        var attributeList = ctx.SyntaxFactory.UnsafeAccessorAttributeList(UnsafeAccessorType.Field, _memberName);
        var returnType = RefType(IdentifierName(_result).AddTrailingSpace())
            .WithRefKeyword(Token(TriviaList(), SyntaxKind.RefKeyword, TriviaList(Space)));

        return PublicStaticExternMethod(returnType, MethodName, parameters, attributeList);
    }
}
