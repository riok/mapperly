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
    private readonly HashSet<ConstructorParameterMapping> _constructorMemberMappings = [];
    private readonly HashSet<MemberAssignmentMapping> _initMemberMappings = [];

    // When set, this mapping should try a DI-provided IMapper<S,T> first.
    // The value is the backing cache field name (e.g. _diMapper_S_T), used to call the helper method.
    internal string? DiMapperCacheFieldName { get; set; }

    // If true, skip emitting DI early-return logic for this mapping (prevents recursion in root user-defined mappings).
    internal bool DisableDiEarlyReturn { get; set; }

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

        // If DI composition is enabled for this mapping, add an early return via a DI-provided mapper.
        if (!DisableDiEarlyReturn && DiMapperCacheFieldName is not null)
        {
            // var diMapper = this.GetOrNull_<cache>();
            var thisExpr = ThisExpression();
            var helperName = "GetOrNull_" + DiMapperCacheFieldName.TrimStart('_');
            var helperCall = ctx.SyntaxFactory.Invocation(Emit.Syntax.SyntaxFactoryHelper.MemberAccess(thisExpr, helperName));

            var diVarName = ctx.NameBuilder.New("diMapper");
            yield return ctx.SyntaxFactory.DeclareLocalVariable(diVarName, helperCall);

            // if (diMapper != null) { return diMapper.Map(source); }
            var diVarId = IdentifierName(diVarName);
            var notNull = Emit.Syntax.SyntaxFactoryHelper.IsNotNull(diVarId);
            var mapCall = ctx.SyntaxFactory.Invocation(Emit.Syntax.SyntaxFactoryHelper.MemberAccess(diVarId, "Map"), ctx.Source);
            var thenReturn = ctx.SyntaxFactory.AddIndentation().Return(mapCall);
            yield return ctx.SyntaxFactory.If(notNull, [thenReturn]);
        }

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
