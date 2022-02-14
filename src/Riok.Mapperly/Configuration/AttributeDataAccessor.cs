using System.Reflection;
using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Configuration;

/// <summary>
/// Creates <see cref="Attribute"/> instances by resolving attribute data from provided symbols.
/// </summary>
internal static class AttributeDataAccessor
{
    public static T? AccessFirstOrDefault<T>(Compilation compilation, ISymbol symbol)
        where T : Attribute
        => Access<T>(compilation, symbol).FirstOrDefault();

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

        var attrDatas = symbol.GetAttributes()
            .Where(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, attrSymbol));

        foreach (var attrData in attrDatas)
        {
            var attr = (T)Activator.CreateInstance(
                attrType,
                BuildConstructorArguments(attrData));

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

    private static object?[] BuildConstructorArguments(AttributeData attrData)
    {
        return attrData.ConstructorArguments
            .Select(arg =>
            {
                if (arg.Value == null)
                    return null;

                // box enum values to resolve correct ctor
                if (arg.Type is not INamedTypeSymbol namedType || namedType.EnumUnderlyingType == null)
                    return arg.Value;

                var assemblyName = arg.Type!.ContainingAssembly.Name;
                var qualifiedTypeName = Assembly.CreateQualifiedName(assemblyName, arg.Type.ToDisplayString());
                return Type.GetType(qualifiedTypeName) is { } type
                    ? Enum.ToObject(type, arg.Value)
                    : arg.Value;
            })
            .ToArray();
    }
}
