using System.Reflection;
using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Configuration;

/// <summary>
/// Creates <see cref="Attribute"/> instances by resolving attribute data from provided symbols.
/// </summary>
internal static class AttributeDataAccessor
{
    public static T? AccessFirstOrDefault<T>(Compilation compilation, ISymbol symbol)
        where T : Attribute => Access<T, T>(compilation, symbol).FirstOrDefault();

    /// <summary>
    /// Reads the attribute data and sets it on a newly created instance of <see cref="TData"/>.
    /// If <see cref="TAttribute"/> has n type parameters,
    /// <see cref="TData"/> needs to have an accessible ctor with the parameters 0 to n-1 to be of type <see cref="ITypeSymbol"/>.
    /// </summary>
    /// <param name="compilation">The compilation.</param>
    /// <param name="symbol">The symbol on which the attributes should be read.</param>
    /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
    /// <typeparam name="TData">The type of the data class. If no type parameters are involved, this is usually the same as <see cref="TAttribute"/>.</typeparam>
    /// <returns>The attribute data.</returns>
    /// <exception cref="InvalidOperationException">If a property or ctor argument of <see cref="TData"/> could not be read on the attribute.</exception>
    public static IEnumerable<TData> Access<TAttribute, TData>(Compilation compilation, ISymbol symbol)
        where TAttribute : Attribute
    {
        var attrType = typeof(TAttribute);
        var attrSymbol = compilation.GetTypeByMetadataName($"{attrType.Namespace}.{attrType.Name}");
        if (attrSymbol == null)
            yield break;

        var attrDatas = symbol
            .GetAttributes()
            .Where(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass?.ConstructedFrom ?? x.AttributeClass, attrSymbol));

        foreach (var attrData in attrDatas)
        {
            var typeArguments = attrData.AttributeClass?.TypeArguments ?? Enumerable.Empty<ITypeSymbol>();
            var ctorArguments = attrData.ConstructorArguments.Select(BuildArgumentValue);
            var newInstanceArguments = typeArguments.Concat(ctorArguments).ToArray();
            var attr = (TData)Activator.CreateInstance(typeof(TData), newInstanceArguments);

            foreach (var namedArgument in attrData.NamedArguments)
            {
                var prop = attrType.GetProperty(namedArgument.Key);
                if (prop == null)
                    throw new InvalidOperationException($"Could not get property {namedArgument.Key} of attribute {attrType.FullName}");

                prop.SetValue(attr, BuildArgumentValue(namedArgument.Value));
            }

            yield return attr;
        }
    }

    private static object? BuildArgumentValue(TypedConstant arg)
    {
        return arg.Kind switch
        {
            _ when arg.IsNull => null,
            TypedConstantKind.Enum => GetEnumValue(arg),
            TypedConstantKind.Array => BuildArrayValue(arg),
            TypedConstantKind.Primitive => arg.Value,
            TypedConstantKind.Type => arg.Value,
            _
                => throw new ArgumentOutOfRangeException(
                    $"{nameof(AttributeDataAccessor)} does not support constructor arguments of kind {arg.Kind.ToString()}"
                ),
        };
    }

    private static object?[] BuildArrayValue(TypedConstant arg)
    {
        var arrayTypeSymbol =
            arg.Type as IArrayTypeSymbol
            ?? throw new InvalidOperationException("Array typed constant is not of type " + nameof(IArrayTypeSymbol));

        var elementType = GetReflectionType(arrayTypeSymbol.ElementType);

        var values = arg.Values.Select(BuildArgumentValue).ToArray();
        var typedValues = Array.CreateInstance(elementType, values.Length);
        Array.Copy(values, typedValues, values.Length);
        return (object?[])typedValues;
    }

    private static object? GetEnumValue(TypedConstant arg)
    {
        var enumType = GetReflectionType(arg.Type ?? throw new InvalidOperationException("Type is null"));
        // if we can't get the enum type then it's not available to reflection (only accessible by Roslyn) so return the TypedConstant
        if (enumType == null)
            return arg;
        return arg.Value == null ? null : Enum.ToObject(enumType, arg.Value);
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
