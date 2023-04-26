using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// An object mapping creating the target instance via a new() call,
/// mapping properties via ctor, object initializer but not by assigning.
/// <seealso cref="NewInstanceObjectMemberMethodMapping"/>
/// </summary>
public class NewInstanceObjectMemberMapping : TypeMapping, INewInstanceObjectMemberMapping
{
    private readonly HashSet<ConstructorParameterMapping> _constructorPropertyMappings = new();
    private readonly HashSet<MemberAssignmentMapping> _initPropertyMappings = new();

    public NewInstanceObjectMemberMapping(ITypeSymbol sourceType, ITypeSymbol targetType)
        : base(sourceType, targetType) { }

    public void AddConstructorParameterMapping(ConstructorParameterMapping mapping) => _constructorPropertyMappings.Add(mapping);

    public void AddInitMemberMapping(MemberAssignmentMapping mapping) => _initPropertyMappings.Add(mapping);

    public override ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        // new T(ctorArgs) { ... };
        var ctorArgs = _constructorPropertyMappings.Select(x => x.BuildArgument(ctx)).ToArray();
        var objectCreationExpression = CreateInstance(TargetType, ctorArgs);

        // add initializer
        if (_initPropertyMappings.Count > 0)
        {
            var initMappings = _initPropertyMappings.Select(x => x.BuildExpression(ctx, null)).ToArray();
            objectCreationExpression = objectCreationExpression.WithInitializer(ObjectInitializer(initMappings));
        }

        return objectCreationExpression;
    }
}
