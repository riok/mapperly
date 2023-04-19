using System.Reflection;
using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Configuration;

/// <summary>
/// Creates <see cref="Attribute"/> instances by resolving attribute data from provided symbols.
/// </summary>
internal static class AttributeDataAccessor
{
    public static T? AccessFirstOrDefault<T>(Compilation compilation, ISymbol symbol)
        where T : Attribute => Access<T>(compilation, symbol).FirstOrDefault();

    public static IEnumerable<T> Access<T>(Compilation compilation, ISymbol symbol)
        where T : Attribute
    {
        var attrType = typeof(T);
        var attrFullName = attrType.FullName;
        if (attrFullName == null)
            yield break;

        var attrSymbol = compilation.GetTypeByMetadataName(attrFullName);
        if (attrSymbol == null)
            yield break;

        var attrDatas = symbol.GetAttributes().Where(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, attrSymbol));

        foreach (var attrData in attrDatas)
        {
            var attr = (T)Activator.CreateInstance(attrType, BuildArgumentValues(attrData.ConstructorArguments).ToArray());

            foreach (var namedArgument in attrData.NamedArguments)
            {
                var prop = attrType.GetProperty(namedArgument.Key);
                if (prop == null)
                    throw new InvalidOperationException($"Could not get property {namedArgument.Key} of attribute {attrType.FullName}");

                prop.SetValue(attr, namedArgument.Value.Value);
            }

            yield return attr;
        }
    }

    private static IEnumerable<object?> BuildArgumentValues(IEnumerable<TypedConstant> values)
    {
        return values.Select(
            arg =>
                arg.Kind switch
                {
                    _ when arg.IsNull => null,
                    TypedConstantKind.Enum => GetEnumValue(arg),
                    TypedConstantKind.Array => BuildArrayValue(arg),
                    TypedConstantKind.Primitive => arg.Value,
                    _
                        => throw new ArgumentOutOfRangeException(
                            $"{nameof(AttributeDataAccessor)} does not support constructor arguments of kind {arg.Kind.ToString()}"
                        ),
                }
        );
    }

    private static object?[] BuildArrayValue(TypedConstant arg)
    {
        var arrayTypeSymbol =
            arg.Type as IArrayTypeSymbol
            ?? throw new InvalidOperationException("Array typed constant is not of type " + nameof(IArrayTypeSymbol));

        var elementType = GetReflectionType(arrayTypeSymbol.ElementType);

        var values = BuildArgumentValues(arg.Values).ToArray();
        var typedValues = Array.CreateInstance(elementType, values.Length);
        Array.Copy(values, typedValues, values.Length);
        return (object?[])typedValues;
    }

    private static object? GetEnumValue(TypedConstant arg)
    {
        var enumType = GetReflectionType(arg.Type ?? throw new InvalidOperationException("Type is null"));
        return arg.Value == null ? null : Enum.ToObject(enumType, arg.Value);
    }

    private static Type GetReflectionType(ITypeSymbol type)
    {
        // other special types not yet supported since they are not used yet.
        if (type.SpecialType == SpecialType.System_String)
            return typeof(string);

        var assemblyName = type.ContainingAssembly.Name;
        var qualifiedTypeName = Assembly.CreateQualifiedName(assemblyName, type.ToDisplayString());
        return Type.GetType(qualifiedTypeName) ?? throw new InvalidOperationException($"Type {qualifiedTypeName} not found");
    }
}
