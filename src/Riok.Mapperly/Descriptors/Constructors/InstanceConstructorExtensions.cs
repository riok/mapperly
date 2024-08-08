using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings;
using Riok.Mapperly.Emit;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Descriptors.Constructors;

public static class InstanceConstructorExtensions
{
    public static ExpressionSyntax CreateInstance(this IInstanceConstructor ctor, TypeMappingBuildContext ctx) =>
        ctor.CreateInstance(ctx, []);

    public static ExpressionSyntax CreateInstance(
        this IInstanceConstructor ctor,
        TypeMappingBuildContext ctx,
        IEnumerable<ExpressionSyntax> args
    ) => ctor.CreateInstance(ctx, args.Select(Argument));

    public static IEnumerable<StatementSyntax> CreateTargetInstance(
        this IInstanceConstructor ctor,
        TypeMappingBuildContext ctx,
        IMapping mapping,
        string targetVariableName,
        bool enableReferenceHandling,
        IReadOnlyCollection<ConstructorParameterMapping> ctorParametersMappings,
        IReadOnlyCollection<MemberAssignmentMapping>? initMemberMappings = null
    )
    {
        if (enableReferenceHandling)
        {
            // TryGetReference
            yield return ReferenceHandlingSyntaxFactoryHelper.TryGetReference(ctx, mapping);
        }

        // new T(ctorArgs) { ... };
        var objectCreationExpression = ctor.CreateInstance(ctx, ctorParametersMappings, initMemberMappings);

        // var target = new T() { ... };
        yield return ctx.SyntaxFactory.DeclareLocalVariable(targetVariableName, objectCreationExpression);

        // set the reference as soon as it is created,
        // as property mappings could refer to the same instance.
        if (enableReferenceHandling)
        {
            // SetReference
            yield return ctx.SyntaxFactory.ExpressionStatement(
                ReferenceHandlingSyntaxFactoryHelper.SetReference(mapping, ctx, IdentifierName(targetVariableName))
            );
        }
    }

    public static ExpressionSyntax CreateInstance(
        this IInstanceConstructor ctor,
        TypeMappingBuildContext ctx,
        IReadOnlyCollection<ConstructorParameterMapping> ctorParametersMappings,
        IReadOnlyCollection<MemberAssignmentMapping>? initMemberMappings = null
    )
    {
        InitializerExpressionSyntax? initializer = null;
        if (initMemberMappings is { Count: > 0 })
        {
            var initPropertiesContext = ctx.AddIndentation();
            var initMappings = initMemberMappings.Select(x => x.BuildExpression(initPropertiesContext, null)).ToArray();
            initializer = ctx.SyntaxFactory.ObjectInitializer(initMappings);
        }

        // new T(ctorArgs) { ... };
        var ctorArgs = ctorParametersMappings.Select(x => x.BuildArgument(ctx)).ToArray();
        return ctor.CreateInstance(ctx, ctorArgs, initializer);
    }
}
