using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Constructors;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// An object mapping creating the target instance via a new() call,
/// mapping properties via ctor, object initializer and by assigning.
/// </summary>
public class NewInstanceObjectMemberMethodMapping(ITypeSymbol sourceType, ITypeSymbol targetType, bool enableReferenceHandling)
    : ObjectMemberMethodMapping(sourceType, targetType),
        INewInstanceObjectMemberMapping
{
    private const string TargetVariableName = "target";

    private IInstanceConstructor? _constructor;
    private readonly HashSet<ConstructorParameterMapping> _constructorMemberMappings = new();
    private readonly HashSet<MemberAssignmentMapping> _initMemberMappings = new();

    public IInstanceConstructor Constructor
    {
        get => _constructor ?? throw new InvalidOperationException("constructor is not set");
        set => _constructor = value;
    }

    public bool HasConstructor => _constructor != null;

    public void AddConstructorParameterMapping(ConstructorParameterMapping mapping) => _constructorMemberMappings.Add(mapping);

    public void AddInitMemberMapping(MemberAssignmentMapping mapping) => _initMemberMappings.Add(mapping);

    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        var targetVariableName = ctx.NameBuilder.New(TargetVariableName);

        // create target instance
        foreach (var statement in CreateTargetInstance(ctx, targetVariableName))
        {
            yield return statement;
        }

        // map properties
        foreach (var expression in BuildBody(ctx, IdentifierName(targetVariableName)))
        {
            yield return expression;
        }

        // return target;
        yield return ctx.SyntaxFactory.ReturnVariable(targetVariableName);
    }

    private IEnumerable<StatementSyntax> CreateTargetInstance(TypeMappingBuildContext ctx, string targetVariableName)
    {
        return Constructor.CreateTargetInstance(
            ctx,
            this,
            targetVariableName,
            enableReferenceHandling,
            _constructorMemberMappings,
            _initMemberMappings
        );
    }
}
