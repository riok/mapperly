using System.Reflection;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Configuration;

/// <summary>
/// Creates <see cref="Attribute"/> instances by resolving attribute data from provided symbols.
/// </summary>
internal class AttributeDataAccessor
{
    private readonly SymbolAccessor _symbolAccessor;

    public AttributeDataAccessor(SymbolAccessor symbolAccessor)
    {
        _symbolAccessor = symbolAccessor;
    }

    public T AccessSingle<T>(ISymbol symbol)
        where T : Attribute => Access<T, T>(symbol).Single();

    public TData? AccessFirstOrDefault<TAttribute, TData>(ISymbol symbol)
        where TAttribute : Attribute => Access<TAttribute, TData>(symbol).FirstOrDefault();

    public IEnumerable<TAttribute> Access<TAttribute>(ISymbol symbol)
        where TAttribute : Attribute => Access<TAttribute, TAttribute>(symbol);

    /// <summary>
    /// Reads the attribute data and sets it on a newly created instance of <see cref="TData"/>.
    /// If <see cref="TAttribute"/> has n type parameters,
    /// <see cref="TData"/> needs to have an accessible ctor with the parameters 0 to n-1 to be of type <see cref="ITypeSymbol"/>.
    /// <see cref="TData"/> needs to have exactly the same constructors as <see cref="TAttribute"/> with additional type arguments.
    /// </summary>
    /// <param name="symbol">The symbol on which the attributes should be read.</param>
    /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
    /// <typeparam name="TData">The type of the data class. If no type parameters are involved, this is usually the same as <see cref="TAttribute"/>.</typeparam>
    /// <returns>The attribute data.</returns>
    /// <exception cref="InvalidOperationException">If a property or ctor argument of <see cref="TData"/> could not be read on the attribute.</exception>
    public IEnumerable<TData> Access<TAttribute, TData>(ISymbol symbol)
        where TAttribute : Attribute
    {
        var attrType = typeof(TAttribute);
        var dataType = typeof(TData);

        var attrDatas = _symbolAccessor.GetAttributes<TAttribute>(symbol);

        foreach (var attrData in attrDatas)
        {
            var typeArguments = (IReadOnlyCollection<ITypeSymbol>?)attrData.AttributeClass?.TypeArguments ?? Array.Empty<ITypeSymbol>();
            var attr = Create<TData>(typeArguments, attrData.ConstructorArguments);

            foreach (var namedArgument in attrData.NamedArguments)
            {
                var prop = dataType.GetProperty(namedArgument.Key);
                if (prop == null)
                    throw new InvalidOperationException($"Could not get property {namedArgument.Key} of attribute {attrType.FullName}");

                prop.SetValue(attr, BuildArgumentValue(namedArgument.Value, prop.PropertyType));
            }

            yield return attr;
        }
    }

    private TData Create<TData>(IReadOnlyCollection<ITypeSymbol> typeArguments, IReadOnlyCollection<TypedConstant> constructorArguments)
    {
        // The data class should have a constructor
        // with generic type parameters of the attribute class
        // as ITypeSymbol parameters followed by all other parameters
        // of the attribute constructor.
        // Multiple attribute class constructors/generic data classes are not yet supported.
        var argCount = typeArguments.Count + constructorArguments.Count;
        foreach (var constructor in typeof(TData).GetConstructors())
        {
            var parameters = constructor.GetParameters();
            if (parameters.Length != argCount)
                continue;

            var constructorArgumentValues = constructorArguments.Select(
                (arg, i) => BuildArgumentValue(arg, parameters[i + typeArguments.Count].ParameterType)
            );
            var constructorTypeAndValueArguments = typeArguments.Concat(constructorArgumentValues).ToArray();
            return (TData)Activator.CreateInstance(typeof(TData), constructorTypeAndValueArguments);
        }

        throw new InvalidOperationException($"{typeof(TData)} does not have a constructor with {argCount} parameters");
    }

    private static object? BuildArgumentValue(TypedConstant arg, Type targetType)
    {
        return arg.Kind switch
        {
            _ when arg.IsNull => null,
            _ when targetType == typeof(StringMemberPath) => CreateMemberPath(arg),
            TypedConstantKind.Enum => GetEnumValue(arg, targetType),
            TypedConstantKind.Array => BuildArrayValue(arg, targetType),
            TypedConstantKind.Primitive => arg.Value,
            TypedConstantKind.Type when targetType == typeof(ITypeSymbol) => arg.Value,
            _
                => throw new ArgumentOutOfRangeException(
                    $"{nameof(AttributeDataAccessor)} does not support constructor arguments of kind {arg.Kind.ToString()} or cannot convert it to {targetType}"
                ),
        };
    }

    private static StringMemberPath CreateMemberPath(TypedConstant arg)
    {
        if (arg.Kind == TypedConstantKind.Array)
        {
            var values = arg.Values.Select(x => (string?)BuildArgumentValue(x, typeof(string))).WhereNotNull().ToList();
            return new StringMemberPath(values);
        }

        if (arg is { Kind: TypedConstantKind.Primitive, Value: string v })
        {
            return new StringMemberPath(v.Split(StringMemberPath.PropertyAccessSeparator));
        }

        throw new InvalidOperationException($"Cannot create {nameof(StringMemberPath)} from {arg.Kind}");
    }

    private static object?[] BuildArrayValue(TypedConstant arg, Type targetType)
    {
        if (!targetType.IsGenericType || targetType.GetGenericTypeDefinition() != typeof(IReadOnlyCollection<>))
            throw new InvalidOperationException($"{nameof(IReadOnlyCollection<object>)} is the only supported array type");

        var elementTargetType = targetType.GetGenericArguments()[0];
        return arg.Values.Select(x => BuildArgumentValue(x, elementTargetType)).ToArray();
    }

    private static object? GetEnumValue(TypedConstant arg, Type targetType)
    {
        if (arg.Value == null)
            return null;

        var enumRoslynType = arg.Type ?? throw new InvalidOperationException("Type is null");
        if (targetType == typeof(IFieldSymbol))
            return enumRoslynType.GetFields().First(f => Equals(f.ConstantValue, arg.Value));

        var enumReflectionType = GetReflectionType(enumRoslynType);
        return enumReflectionType == null
            ? throw new InvalidOperationException(
                $"Could not resolve enum reflection type of {enumRoslynType.Name} or {targetType} is not supported"
            )
            : Enum.ToObject(enumReflectionType, arg.Value);
    }

    private static Type? GetReflectionType(ITypeSymbol type)
    {
        // other special types not yet supported since they are not used yet.
        if (type.SpecialType == SpecialType.System_String)
            return typeof(string);

        var assemblyName = type.ContainingAssembly.Name;
        var qualifiedTypeName = Assembly.CreateQualifiedName(assemblyName, type.ToDisplayString());
        return Type.GetType(qualifiedTypeName);
    }
}
