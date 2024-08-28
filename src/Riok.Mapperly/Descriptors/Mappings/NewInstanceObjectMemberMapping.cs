using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Constructors;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// An object mapping creating the target instance via a new() call,
/// mapping properties via ctor, object initializer but not by assigning.
/// <seealso cref="NewInstanceObjectMemberMethodMapping"/>
/// </summary>
public class NewInstanceObjectMemberMapping(ITypeSymbol sourceType, ITypeSymbol targetType)
    : NewInstanceMapping(sourceType, targetType),
        INewInstanceObjectMemberMapping
{
    private IInstanceConstructor? _constructor;
    private readonly HashSet<ConstructorParameterMapping> _constructorMemberMappings = [];
    private readonly HashSet<MemberAssignmentMapping> _initMemberMappings = [];

    public IInstanceConstructor Constructor
    {
        get => _constructor ?? throw new InvalidOperationException("constructor is not set");
        set => _constructor = value;
    }

    public bool HasConstructor => _constructor != null;

    public void AddConstructorParameterMapping(ConstructorParameterMapping mapping) => _constructorMemberMappings.Add(mapping);

    public void AddInitMemberMapping(MemberAssignmentMapping mapping) => _initMemberMappings.Add(mapping);

    public override ExpressionSyntax Build(TypeMappingBuildContext ctx) =>
        Constructor.CreateInstance(ctx, _constructorMemberMappings, _initMemberMappings);
}
