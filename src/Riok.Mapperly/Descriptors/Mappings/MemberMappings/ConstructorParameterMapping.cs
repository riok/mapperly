using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings.SourceValue;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.MemberMappings;

public class ConstructorParameterMapping(
    IParameterSymbol parameter,
    ISourceValue sourceValue,
    bool selfOrPreviousIsUnmappedOptional,
    MemberMappingInfo memberInfo
)
{
    public MemberMappingInfo MemberInfo { get; } = memberInfo;

    private readonly bool _selfOrPreviousIsUnmappedOptional = selfOrPreviousIsUnmappedOptional;
    private readonly IParameterSymbol _parameter = parameter;
    private readonly ISourceValue _sourceValue = sourceValue;

    public ArgumentSyntax BuildArgument(TypeMappingBuildContext ctx)
    {
        var argumentExpression = _sourceValue.Build(ctx);
        var arg = Argument(argumentExpression);
        return _selfOrPreviousIsUnmappedOptional ? arg.WithNameColon(SpacedNameColon(_parameter.Name)) : arg;
    }

    protected bool Equals(ConstructorParameterMapping other) =>
        _parameter.Equals(other._parameter, SymbolEqualityComparer.Default)
        && _sourceValue.Equals(other._sourceValue)
        && _selfOrPreviousIsUnmappedOptional == other._selfOrPreviousIsUnmappedOptional;

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (obj.GetType() != GetType())
            return false;

        return Equals((ConstructorParameterMapping)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = SymbolEqualityComparer.Default.GetHashCode(_parameter);
            hashCode = (hashCode * 397) ^ _sourceValue.GetHashCode();
            hashCode = (hashCode * 397) ^ _selfOrPreviousIsUnmappedOptional.GetHashCode();
            return hashCode;
        }
    }

    public static bool operator ==(ConstructorParameterMapping? left, ConstructorParameterMapping? right) => Equals(left, right);

    public static bool operator !=(ConstructorParameterMapping? left, ConstructorParameterMapping? right) => !Equals(left, right);
}
