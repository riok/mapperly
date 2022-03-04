using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Descriptors.Mappings.PropertyMappings;

public class ConstructorParameterMapping
{
    private readonly IParameterSymbol _parameter;
    private readonly NullPropertyMapping _mapping;
    private readonly bool _selfOrPreviousIsUnmappedOptional;

    public ConstructorParameterMapping(
        IParameterSymbol parameter,
        NullPropertyMapping mapping,
        bool selfOrPreviousIsUnmappedOptional)
    {
        _parameter = parameter;
        _mapping = mapping;
        _selfOrPreviousIsUnmappedOptional = selfOrPreviousIsUnmappedOptional;
    }

    public ArgumentSyntax BuildArgument(ExpressionSyntax source)
    {
        var argumentExpression = _mapping.Build(source);
        var arg = Argument(argumentExpression);
        return _selfOrPreviousIsUnmappedOptional
            ? arg.WithNameColon(NameColon(_parameter.Name))
            : arg;
    }

    protected bool Equals(ConstructorParameterMapping other)
        => _parameter.Equals(other._parameter, SymbolEqualityComparer.Default)
            && _mapping.Equals(other._mapping)
            && _selfOrPreviousIsUnmappedOptional == other._selfOrPreviousIsUnmappedOptional;

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((ConstructorParameterMapping)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = SymbolEqualityComparer.Default.GetHashCode(_parameter);
            hashCode = (hashCode * 397) ^ _mapping.GetHashCode();
            hashCode = (hashCode * 397) ^ _selfOrPreviousIsUnmappedOptional.GetHashCode();
            return hashCode;
        }
    }

    public static bool operator ==(ConstructorParameterMapping? left, ConstructorParameterMapping? right) => Equals(left, right);

    public static bool operator !=(ConstructorParameterMapping? left, ConstructorParameterMapping? right) => !Equals(left, right);
}
