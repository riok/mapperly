using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Configuration.MethodReferences;
using Riok.Mapperly.Configuration.PropertyReferences;
using Riok.Mapperly.Descriptors;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Configuration;

/// <summary>
/// Creates <see cref="Attribute"/> instances by resolving attribute data from provided symbols.
/// </summary>
public class AttributeDataAccessor(SymbolAccessor symbolAccessor)
{
    private const char FullNameOfPrefix = '@';

    public static MapperConfiguration ReadMapperDefaultsAttribute(AttributeData attrData)
    {
        return new MapperConfiguration
        {
            AllowNullPropertyAssignment = GetSimpleValue<bool>(attrData, nameof(MapperDefaultsAttribute.AllowNullPropertyAssignment)),
            AutoUserMappings = GetSimpleValue<bool>(attrData, nameof(MapperDefaultsAttribute.AutoUserMappings)),
            EnabledConversions = GetSimpleValue<MappingConversionType>(attrData, nameof(MapperDefaultsAttribute.EnabledConversions)),
            EnumMappingIgnoreCase = GetSimpleValue<bool>(attrData, nameof(MapperDefaultsAttribute.EnumMappingIgnoreCase)),
            EnumMappingStrategy = GetSimpleValue<EnumMappingStrategy>(attrData, nameof(MapperDefaultsAttribute.EnumMappingStrategy)),
            EnumNamingStrategy = GetSimpleValue<EnumNamingStrategy>(attrData, nameof(MapperDefaultsAttribute.EnumNamingStrategy)),
            IgnoreObsoleteMembersStrategy = GetSimpleValue<IgnoreObsoleteMembersStrategy>(
                attrData,
                nameof(MapperDefaultsAttribute.IgnoreObsoleteMembersStrategy)
            ),
            IncludedConstructors = GetSimpleValue<MemberVisibility>(attrData, nameof(MapperDefaultsAttribute.IncludedConstructors)),
            IncludedMembers = GetSimpleValue<MemberVisibility>(attrData, nameof(MapperDefaultsAttribute.IncludedMembers)),
            PreferParameterlessConstructors = GetSimpleValue<bool>(
                attrData,
                nameof(MapperDefaultsAttribute.PreferParameterlessConstructors)
            ),
            PropertyNameMappingStrategy = GetSimpleValue<PropertyNameMappingStrategy>(
                attrData,
                nameof(MapperDefaultsAttribute.PropertyNameMappingStrategy)
            ),
            RequiredEnumMappingStrategy = GetSimpleValue<RequiredMappingStrategy>(
                attrData,
                nameof(MapperDefaultsAttribute.RequiredEnumMappingStrategy)
            ),
            RequiredMappingStrategy = GetSimpleValue<RequiredMappingStrategy>(
                attrData,
                nameof(MapperDefaultsAttribute.RequiredMappingStrategy)
            ),
            ThrowOnMappingNullMismatch = GetSimpleValue<bool>(attrData, nameof(MapperDefaultsAttribute.ThrowOnMappingNullMismatch)),
            ThrowOnPropertyMappingNullMismatch = GetSimpleValue<bool>(
                attrData,
                nameof(MapperDefaultsAttribute.ThrowOnPropertyMappingNullMismatch)
            ),
            UseDeepCloning = GetSimpleValue<bool>(attrData, nameof(MapperDefaultsAttribute.UseDeepCloning)),
            UseReferenceHandling = GetSimpleValue<bool>(attrData, nameof(MapperDefaultsAttribute.UseReferenceHandling)),
        };
    }

    public FormatProviderAttribute ReadFormatProviderAttribute(ISymbol symbol)
    {
        var attrData = GetRequiredAttribute<FormatProviderAttribute>(symbol);

        return new FormatProviderAttribute { Default = GetSimpleValueOrDefault<bool>(attrData, nameof(FormatProviderAttribute.Default)) };
    }

    public MapperConfiguration ReadMapperAttribute(ISymbol symbol)
    {
        var attrData = GetRequiredAttribute<MapperAttribute>(symbol);

        return new MapperConfiguration
        {
            AllowNullPropertyAssignment = GetSimpleValue<bool>(attrData, nameof(MapperAttribute.AllowNullPropertyAssignment)),
            AutoUserMappings = GetSimpleValue<bool>(attrData, nameof(MapperAttribute.AutoUserMappings)),
            EnabledConversions = GetSimpleValue<MappingConversionType>(attrData, nameof(MapperAttribute.EnabledConversions)),
            EnumMappingIgnoreCase = GetSimpleValue<bool>(attrData, nameof(MapperAttribute.EnumMappingIgnoreCase)),
            EnumMappingStrategy = GetSimpleValue<EnumMappingStrategy>(attrData, nameof(MapperAttribute.EnumMappingStrategy)),
            EnumNamingStrategy = GetSimpleValue<EnumNamingStrategy>(attrData, nameof(MapperAttribute.EnumNamingStrategy)),
            IgnoreObsoleteMembersStrategy = GetSimpleValue<IgnoreObsoleteMembersStrategy>(
                attrData,
                nameof(MapperAttribute.IgnoreObsoleteMembersStrategy)
            ),
            IncludedConstructors = GetSimpleValue<MemberVisibility>(attrData, nameof(MapperAttribute.IncludedConstructors)),
            IncludedMembers = GetSimpleValue<MemberVisibility>(attrData, nameof(MapperAttribute.IncludedMembers)),
            PreferParameterlessConstructors = GetSimpleValue<bool>(attrData, nameof(MapperAttribute.PreferParameterlessConstructors)),
            PropertyNameMappingStrategy = GetSimpleValue<PropertyNameMappingStrategy>(
                attrData,
                nameof(MapperAttribute.PropertyNameMappingStrategy)
            ),
            RequiredEnumMappingStrategy = GetSimpleValue<RequiredMappingStrategy>(
                attrData,
                nameof(MapperAttribute.RequiredEnumMappingStrategy)
            ),
            RequiredMappingStrategy = GetSimpleValue<RequiredMappingStrategy>(attrData, nameof(MapperAttribute.RequiredMappingStrategy)),
            ThrowOnMappingNullMismatch = GetSimpleValue<bool>(attrData, nameof(MapperAttribute.ThrowOnMappingNullMismatch)),
            ThrowOnPropertyMappingNullMismatch = GetSimpleValue<bool>(attrData, nameof(MapperAttribute.ThrowOnPropertyMappingNullMismatch)),
            UseDeepCloning = GetSimpleValue<bool>(attrData, nameof(MapperAttribute.UseDeepCloning)),
            UseReferenceHandling = GetSimpleValue<bool>(attrData, nameof(MapperAttribute.UseReferenceHandling)),
        };
    }

    public MapperIgnoreObsoleteMembersAttribute? ReadMapperIgnoreObsoleteMembersAttribute(ISymbol symbol)
    {
        var attrData = GetAttribute<MapperIgnoreObsoleteMembersAttribute>(symbol);
        if (attrData == null)
        {
            return null;
        }

        return new MapperIgnoreObsoleteMembersAttribute(
            GetSimpleValueOrDefault(
                attrData,
                nameof(MapperIgnoreObsoleteMembersAttribute.IgnoreObsoleteStrategy),
                IgnoreObsoleteMembersStrategy.Both
            )
        );
    }

    public IEnumerable<NestedMembersMappingConfiguration> ReadMapNestedPropertiesAttribute(ISymbol symbol)
    {
        var attrDatas = symbolAccessor.GetAttributes<MapNestedPropertiesAttribute>(symbol);
        foreach (var attrData in attrDatas)
        {
            var config = CreateMemberPath(attrData, nameof(NestedMembersMappingConfiguration.Source));
            yield return new NestedMembersMappingConfiguration(config);
        }
    }

    public MapperRequiredMappingAttribute? ReadMapperRequiredMappingAttribute(ISymbol symbol)
    {
        var attrData = GetAttribute<MapperRequiredMappingAttribute>(symbol);
        if (attrData == null)
            return null;

        return new MapperRequiredMappingAttribute(
            GetSimpleValueOrDefault(attrData, nameof(MapperRequiredMappingAttribute.RequiredMappingStrategy), RequiredMappingStrategy.None)
        );
    }

    public EnumMemberAttribute? ReadEnumMemberAttribute(ISymbol symbol)
    {
        var attrData = GetAttribute<EnumMemberAttribute>(symbol);
        if (attrData == null)
            return null;

        return new EnumMemberAttribute { Value = GetSimpleValue(attrData, nameof(EnumMemberAttribute.Value)) };
    }

    public EnumConfiguration? ReadMapEnumAttribute(ISymbol symbol)
    {
        //var oldValue = Access<MapEnumAttribute, EnumConfiguration>(symbol).FirstOrDefault();
        var attrData = GetAttribute<MapEnumAttribute>(symbol);
        if (attrData == null)
            return null;

        return new EnumConfiguration(GetSimpleValueOrDefault<EnumMappingStrategy>(attrData, nameof(MapEnumAttribute.Strategy)))
        {
            NamingStrategy = GetSimpleValueOrDefault(attrData, nameof(MapEnumAttribute.NamingStrategy), EnumNamingStrategy.MemberName),
            FallbackValue = GetAttributeValue(attrData, nameof(MapEnumAttribute.FallbackValue)),
            IgnoreCase = GetSimpleValue<bool>(attrData, nameof(MapEnumAttribute.IgnoreCase)),
        };
    }

    public IEnumerable<EnumValueMappingConfiguration> ReadMapEnumValueAttribute(ISymbol symbol)
    {
        var attrDatas = symbolAccessor.GetAttributes<MapEnumValueAttribute>(symbol);
        foreach (var attrData in attrDatas)
        {
            var source = GetAttributeValue(attrData, nameof(MapEnumValueAttribute.Source));
            if (source == null)
                continue;
            var target = GetAttributeValue(attrData, nameof(MapEnumValueAttribute.Target));
            if (target == null)
                continue;

            yield return new EnumValueMappingConfiguration(source.Value, target.Value);
        }
    }

    public IEnumerable<MapperIgnoreEnumValueConfiguration> ReadMapperIgnoreSourceValueAttribute(ISymbol symbol)
    {
        var attrDatas = symbolAccessor.GetAttributes<MapperIgnoreSourceValueAttribute>(symbol);
        foreach (var attrData in attrDatas)
        {
            var fieldSymbol = GetFieldSymbol(attrData, "source");
            if (fieldSymbol is null)
                continue;

            yield return new MapperIgnoreEnumValueConfiguration(fieldSymbol);
        }
    }

    public IEnumerable<MapperIgnoreEnumValueConfiguration> ReadMapperIgnoreTargetValueAttribute(ISymbol symbol)
    {
        var attrDatas = symbolAccessor.GetAttributes<MapperIgnoreTargetValueAttribute>(symbol);
        foreach (var attrData in attrDatas)
        {
            var fieldSymbol = GetFieldSymbol(attrData, "target");
            if (fieldSymbol is null)
                continue;

            yield return new MapperIgnoreEnumValueConfiguration(fieldSymbol);
        }
    }

    public ComponentModelDescriptionAttributeConfiguration? ReadDescriptionAttribute(ISymbol symbol)
    {
        return Access<DescriptionAttribute, ComponentModelDescriptionAttributeConfiguration>(symbol).FirstOrDefault();
    }

    public UserMappingConfiguration? ReadUserMappingAttribute(ISymbol symbol)
    {
        return Access<UserMappingAttribute, UserMappingConfiguration>(symbol).FirstOrDefault();
    }

    public bool HasUseMapperAttribute(ISymbol symbol)
    {
        return Access<UseMapperAttribute, UseMapperAttribute>(symbol).Any();
    }

    public IEnumerable<MapperIgnoreSourceAttribute> ReadMapperIgnoreSourceAttributes(ISymbol symbol)
    {
        return Access<MapperIgnoreSourceAttribute, MapperIgnoreSourceAttribute>(symbol);
    }

    public IEnumerable<MapperIgnoreTargetAttribute> ReadMapperIgnoreTargetAttributes(ISymbol symbol)
    {
        return Access<MapperIgnoreTargetAttribute, MapperIgnoreTargetAttribute>(symbol);
    }

    public IEnumerable<MemberValueMappingConfiguration> ReadMapValueAttribute(ISymbol symbol)
    {
        return Access<MapValueAttribute, MemberValueMappingConfiguration>(symbol);
    }

    public IEnumerable<MemberMappingConfiguration> ReadMapPropertyAttributes(ISymbol symbol)
    {
        return Access<MapPropertyAttribute, MemberMappingConfiguration>(symbol);
    }

    public IEnumerable<IncludeMappingConfiguration> ReadIncludeMappingConfigurationAttributes(ISymbol symbol)
    {
        return Access<IncludeMappingConfigurationAttribute, IncludeMappingConfiguration>(symbol);
    }

    public IEnumerable<DerivedTypeMappingConfiguration> ReadMapDerivedTypeAttributes(ISymbol symbol)
    {
        return Access<MapDerivedTypeAttribute, DerivedTypeMappingConfiguration>(symbol);
    }

    public IEnumerable<DerivedTypeMappingConfiguration> ReadGenericMapDerivedTypeAttributes(ISymbol symbol)
    {
        return Access<MapDerivedTypeAttribute<object, object>, DerivedTypeMappingConfiguration>(symbol);
    }

    public IEnumerable<MemberMappingConfiguration> ReadMapPropertyFromSourceAttributes(ISymbol symbol)
    {
        return Access<MapPropertyFromSourceAttribute, MemberMappingConfiguration>(symbol);
    }

    public IEnumerable<UseStaticMapperConfiguration> ReadUseStaticMapperAttributes(ISymbol symbol)
    {
        return Access<UseStaticMapperAttribute, UseStaticMapperConfiguration>(symbol);
    }

    public IEnumerable<UseStaticMapperConfiguration> ReadGenericUseStaticMapperAttributes(ISymbol symbol)
    {
        return Access<UseStaticMapperAttribute<object>, UseStaticMapperConfiguration>(symbol);
    }

    public string GetMappingName(IMethodSymbol methodSymbol)
    {
        return Access<NamedMappingAttribute, NamedMappingAttribute>(methodSymbol).Select(e => e.Name).FirstOrDefault() ?? methodSymbol.Name;
    }

    public bool IsMappingNameEqualTo(IMethodSymbol methodSymbol, string name)
    {
        return string.Equals(GetMappingName(methodSymbol), name, StringComparison.Ordinal);
    }

    internal IEnumerable<NotNullIfNotNullAttribute> TryReadNotNullIfNotNullAttributes(IMethodSymbol symbol)
    {
        return TryAccess<NotNullIfNotNullAttribute, NotNullIfNotNullAttribute>(symbol.GetReturnTypeAttributes());
    }

    private IEnumerable<TData> Access<TAttribute, TData>(ISymbol symbol)
        where TAttribute : Attribute
        where TData : notnull
    {
        var attrDatas = symbolAccessor.GetAttributes<TAttribute>(symbol);
        return Access<TAttribute, TData>(attrDatas);
    }

    private IEnumerable<TData> TryAccess<TAttribute, TData>(IEnumerable<AttributeData> attributes)
        where TAttribute : Attribute
        where TData : notnull
    {
        var attrDatas = symbolAccessor.TryGetAttributes<TAttribute>(attributes);
        return attrDatas.Select(a => Access<TAttribute, TData>(a));
    }

    /// <summary>
    /// Reads the attribute data and sets it on a newly created instance of <see cref="TData"/>.
    /// If <see cref="TAttribute"/> has n type parameters,
    /// <see cref="TData"/> needs to have an accessible ctor with the parameters 0 to n-1 to be of type <see cref="ITypeSymbol"/>.
    /// <see cref="TData"/> needs to have exactly the same constructors as <see cref="TAttribute"/> with additional type arguments.
    /// </summary>
    /// <param name="attributes">The attributes data.</param>
    /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
    /// <typeparam name="TData">The type of the data class. If no type parameters are involved, this is usually the same as <see cref="TAttribute"/>.</typeparam>
    /// <returns>The attribute data.</returns>
    /// <exception cref="InvalidOperationException">If a property or ctor argument of <see cref="TData"/> could not be read on the attribute.</exception>
    private IEnumerable<TData> Access<TAttribute, TData>(IEnumerable<AttributeData> attributes)
        where TAttribute : Attribute
        where TData : notnull
    {
        foreach (var attrData in symbolAccessor.GetAttributes<TAttribute>(attributes))
        {
            yield return Access<TAttribute, TData>(attrData, symbolAccessor);
        }
    }

    private static TData Access<TAttribute, TData>(AttributeData attrData, SymbolAccessor? symbolAccessor = null)
        where TAttribute : Attribute
        where TData : notnull
    {
        var attrType = typeof(TAttribute);
        var dataType = typeof(TData);

        var syntax = (AttributeSyntax?)attrData.ApplicationSyntaxReference?.GetSyntax();
        var syntaxArguments =
            (IReadOnlyList<AttributeArgumentSyntax>?)syntax?.ArgumentList?.Arguments
            ?? new AttributeArgumentSyntax[attrData.ConstructorArguments.Length + attrData.NamedArguments.Length];
        var typeArguments = (IReadOnlyCollection<ITypeSymbol>?)attrData.AttributeClass?.TypeArguments ?? [];
        var attr = Create<TData>(typeArguments, attrData.ConstructorArguments, syntaxArguments, symbolAccessor);

        var syntaxIndex = attrData.ConstructorArguments.Length;
        var propertiesByName = dataType.GetProperties().GroupBy(x => x.Name).ToDictionary(x => x.Key, x => x.First());
        foreach (var namedArgument in attrData.NamedArguments)
        {
            if (!propertiesByName.TryGetValue(namedArgument.Key, out var prop))
                throw new InvalidOperationException($"Could not get property {namedArgument.Key} of attribute {attrType.FullName}");

            var value = BuildArgumentValue(namedArgument.Value, prop.PropertyType, syntaxArguments[syntaxIndex], symbolAccessor);
            prop.SetValue(attr, value);
            syntaxIndex++;
        }

        if (attr is HasSyntaxReference symbolRefHolder)
        {
            symbolRefHolder.SyntaxReference = attrData.ApplicationSyntaxReference?.GetSyntax();
        }

        return attr;
    }

    private static TData Create<TData>(
        IReadOnlyCollection<ITypeSymbol> typeArguments,
        IReadOnlyCollection<TypedConstant> constructorArguments,
        IReadOnlyList<AttributeArgumentSyntax> argumentSyntax,
        SymbolAccessor? symbolAccessor
    )
        where TData : notnull
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
                (arg, i) => BuildArgumentValue(arg, parameters[i + typeArguments.Count].ParameterType, argumentSyntax[i], symbolAccessor)
            );
            var constructorTypeAndValueArguments = typeArguments.Concat(constructorArgumentValues).ToArray();
            if (!ValidateParameterTypes(constructorTypeAndValueArguments, parameters))
                continue;

            return (TData?)Activator.CreateInstance(typeof(TData), constructorTypeAndValueArguments)
                ?? throw new InvalidOperationException($"Could not create instance of {typeof(TData)}");
        }

        throw new InvalidOperationException(
            $"{typeof(TData)} does not have a constructor with {argCount} parameters and matchable arguments"
        );
    }

    private static object? BuildArgumentValue(
        TypedConstant arg,
        Type targetType,
        AttributeArgumentSyntax? syntax,
        SymbolAccessor? symbolAccessor
    )
    {
        return arg.Kind switch
        {
            _ when (targetType == typeof(AttributeValue?) || targetType == typeof(AttributeValue)) && syntax != null => new AttributeValue(
                arg,
                syntax.Expression
            ),
            _ when arg.IsNull => null,
            _ when targetType == typeof(IMemberPathConfiguration) => CreateMemberPath(arg, syntax, symbolAccessor),
            _ when targetType == typeof(IMethodReferenceConfiguration) => CreateMethodReference(arg, syntax, symbolAccessor),
            TypedConstantKind.Enum => GetEnumValue(arg, targetType),
            TypedConstantKind.Array => BuildArrayValue(arg, targetType, symbolAccessor),
            TypedConstantKind.Primitive => arg.Value,
            TypedConstantKind.Type when targetType == typeof(ITypeSymbol) => arg.Value,
            _ => throw new ArgumentOutOfRangeException(
                $"{nameof(AttributeDataAccessor)} does not support constructor arguments of kind {arg.Kind.ToString()} or cannot convert it to {targetType}"
            ),
        };
    }

    private static IMemberPathConfiguration CreateMemberPath(
        TypedConstant arg,
        AttributeArgumentSyntax? syntax,
        SymbolAccessor? symbolAccessor
    )
    {
        ThrowHelpers.ThrowIfNull(symbolAccessor);

        if (arg.Kind == TypedConstantKind.Array)
        {
            var values = arg
                .Values.Select(x => (string?)BuildArgumentValue(x, typeof(string), null, symbolAccessor))
                .WhereNotNull()
                .ToImmutableEquatableArray();
            return new StringMemberPath(values);
        }

        if (arg.Kind == TypedConstantKind.Primitive && syntax.TryGetNameOfSyntax(out var invocationExpressionSyntax))
        {
            return CreateNameOfMemberPath(invocationExpressionSyntax, symbolAccessor);
        }

        if (arg is { Kind: TypedConstantKind.Primitive, Value: string v })
        {
            return new StringMemberPath(v.Split(MemberPathConstants.MemberAccessSeparator).ToImmutableEquatableArray());
        }

        throw new InvalidOperationException($"Cannot create {nameof(StringMemberPath)} from {arg.Kind}");
    }

    private static IMemberPathConfiguration CreateNameOfMemberPath(InvocationExpressionSyntax nameofSyntax, SymbolAccessor symbolAccessor)
    {
        // @ prefix opts-in to full nameof
        var fullNameOf = nameofSyntax.IsFullNameOfSyntax();

        var nameOfOperation = symbolAccessor.GetOperation<INameOfOperation>(nameofSyntax);
        var memberRefOperation = nameOfOperation?.GetFirstChildOperation<IMemberReferenceOperation>();
        if (memberRefOperation == null)
        {
            // fall back to old skip-first-segment approach
            // to ensure backwards compability.

            var argMemberPathStr = nameofSyntax.ArgumentList.Arguments[0].ToFullString();
            var argMemberPath = argMemberPathStr
                .TrimStart(FullNameOfPrefix)
                .Split(MemberPathConstants.MemberAccessSeparator)
                .Skip(1)
                .ToImmutableEquatableArray();
            return new StringMemberPath(argMemberPath);
        }

        var memberPath = new List<ISymbol>();
        while (memberRefOperation != null)
        {
            memberPath.Add(memberRefOperation.Member);
            memberRefOperation = memberRefOperation.GetFirstChildOperation<IMemberReferenceOperation>();

            // if not fullNameOf only consider the last member path segment
            if (!fullNameOf && memberPath.Count > 1)
                break;
        }

        memberPath.Reverse();
        return new SymbolMemberPath(memberPath.ToImmutableEquatableArray());
    }

    private static IMethodReferenceConfiguration CreateMethodReference(
        TypedConstant arg,
        AttributeArgumentSyntax? syntax,
        SymbolAccessor? symbolAccessor
    )
    {
        ThrowHelpers.ThrowIfNull(symbolAccessor);

        if (arg.Kind != TypedConstantKind.Primitive)
        {
            throw new InvalidOperationException($"Cannot create {nameof(IMethodReferenceConfiguration)} from {arg.Kind}");
        }

        if (
            syntax.TryGetNameOfSyntax(out var invocationExpressionSyntax)
            && invocationExpressionSyntax.IsFullNameOfSyntax()
            && TryCreateNameOfMethodReferenceConfiguration(invocationExpressionSyntax, symbolAccessor, out var configuration)
        )
        {
            return configuration;
        }

        if (arg.Value is not string fullName)
        {
            throw new InvalidOperationException($"Unknown method reference configuration: {arg.Value}");
        }

        var splitPoint = fullName.LastIndexOf(MemberPathConstants.MemberAccessSeparator);
        var methodName = splitPoint == -1 ? fullName : fullName[(splitPoint + 1)..];
        var targetName = splitPoint == -1 ? null : fullName[..splitPoint];
        return new StringMethodReferenceConfiguration(methodName, targetName, fullName);
    }

    private static bool TryCreateNameOfMethodReferenceConfiguration(
        InvocationExpressionSyntax nameofSyntax,
        SymbolAccessor symbolAccessor,
        [NotNullWhen(true)] out IMethodReferenceConfiguration? configuration
    )
    {
        configuration = null;
        var nameOfOperation = symbolAccessor.GetOperation<INameOfOperation>(nameofSyntax);
        var operation = nameOfOperation?.GetFirstChildOperation<IOperation>();
        var memberName = operation?.Syntax.TryGetInferredMemberName();
        if (memberName is null || operation is null)
        {
            return false;
        }

        operation = operation.GetFirstChildOperation<IOperation>();
        if (operation is null)
        {
            return false;
        }

        if (operation is IInvalidOperation)
        {
            var targetName = operation.Syntax.ToString();
            configuration = new StringMethodReferenceConfiguration(memberName, targetName, $"{targetName}.{memberName}");
            return true;
        }

        if (operation.Type is not INamedTypeSymbol typeSymbol)
        {
            return false;
        }

        var field = operation.GetMemberSymbol();
        if (field is null)
        {
            configuration = new ExternalStaticMethodReferenceConfiguration(memberName, typeSymbol);
            return true;
        }

        configuration = new ExternalInstanceMethodReferenceConfiguration(memberName, field, typeSymbol);
        return true;
    }

    private static object?[] BuildArrayValue(TypedConstant arg, Type targetType, SymbolAccessor? symbolAccessor)
    {
        if (!targetType.IsGenericType || targetType.GetGenericTypeDefinition() != typeof(IReadOnlyCollection<>))
            throw new InvalidOperationException($"{nameof(IReadOnlyCollection<object>)} is the only supported array type");

        var elementTargetType = targetType.GetGenericArguments()[0];
        return arg.Values.Select(x => BuildArgumentValue(x, elementTargetType, null, symbolAccessor)).ToArray();
    }

    private static object? GetEnumValue(TypedConstant arg, Type targetType)
    {
        if (arg.Value == null)
            return null;

        var enumRoslynType = arg.Type ?? throw new InvalidOperationException("Type is null");
        if (targetType == typeof(IFieldSymbol))
            return enumRoslynType.GetFields().First(f => Equals(f.ConstantValue, arg.Value));

        if (targetType.IsConstructedGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            targetType = Nullable.GetUnderlyingType(targetType)!;
        }

        return Enum.ToObject(targetType, arg.Value);
    }

    private static bool ValidateParameterTypes(object?[] arguments, ParameterInfo[] parameters)
    {
        if (arguments.Length != parameters.Length)
            return false;

        for (var argIdx = 0; argIdx < arguments.Length; argIdx++)
        {
            var value = arguments[argIdx];
            var param = parameters[argIdx];
            if (value == null && param.ParameterType.IsValueType)
                return false;

            if (value?.GetType().IsAssignableTo(param.ParameterType) == false)
                return false;
        }

        return true;
    }

    private AttributeData? GetAttribute<TAttribute>(ISymbol symbol)
        where TAttribute : Attribute
    {
        return symbolAccessor.GetAttributes<TAttribute>(symbol).FirstOrDefault();
    }

    private AttributeData GetRequiredAttribute<TAttribute>(ISymbol symbol)
        where TAttribute : Attribute
    {
        return GetAttribute<TAttribute>(symbol)
            ?? throw new InvalidOperationException($"Could not find attribute {typeof(TAttribute).FullName} on {symbol.Name}");
    }

    private IMemberPathConfiguration CreateMemberPath(AttributeData attrData, string name)
    {
        if (TryGetTypedConstantWithAttributeArgumentSyntax(attrData, name, out var typedConstant, out var argumentSyntax))
        {
            return CreateMemberPath(typedConstant.Value, argumentSyntax, symbolAccessor);
        }

        return new StringMemberPath(name.Split(MemberPathConstants.MemberAccessSeparator).ToImmutableEquatableArray());
    }

    private static string? GetSimpleValue(AttributeData attrData, string propertyName)
    {
        var typedConstant = GetTypedConstant(attrData, propertyName);
        return typedConstant?.Value as string;
    }

    private static TValue GetSimpleValueOrDefault<TValue>(AttributeData attrData, string propertyName, TValue defaultValue = default)
        where TValue : struct
    {
        return GetSimpleValue<TValue>(attrData, propertyName) ?? defaultValue;
    }

    public static IFieldSymbol? GetFieldSymbol(AttributeData attrData, string propertyName)
    {
        var nullableTypedConstant = GetTypedConstant(attrData, propertyName);
        if (nullableTypedConstant is not { } typedConstant)
        {
            return null;
        }

        var roslynType = typedConstant.Type;
        return roslynType?.GetFields().FirstOrDefault(f => Equals(f.ConstantValue, typedConstant.Value));
    }

    private static TValue? GetSimpleValue<TValue>(AttributeData attrData, string propertyName)
        where TValue : struct
    {
        var nullableTypedConstant = GetTypedConstant(attrData, propertyName);
        if (nullableTypedConstant is not { } typedConstant)
        {
            return null;
        }

        var value = typedConstant.Value;

        if (value is null)
            return null;

        if (typeof(TValue).IsEnum && value is int i)
        {
            return (TValue)(object)i;
        }

        if (value is TValue tValue)
        {
            return tValue;
        }

        return null;
    }

    private static bool TryGetTypedConstantWithAttributeArgumentSyntax(
        AttributeData attrData,
        string propertyName,
        [NotNullWhen(true)] out TypedConstant? typedConstant,
        [NotNullWhen(true)] out AttributeArgumentSyntax? syntax
    )
    {
        syntax = null;
        typedConstant = null;
        var syntaxes = (AttributeSyntax?)attrData.ApplicationSyntaxReference?.GetSyntax();
        if (syntaxes?.ArgumentList?.Arguments is not { } argumentSyntaxes)
        {
            return false;
        }

        var constructorParameters = attrData.AttributeConstructor?.Parameters;
        if (constructorParameters is not null)
        {
            foreach (var parameter in constructorParameters)
            {
                if (!string.Equals(parameter.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var ordinal = parameter.Ordinal;
                if (ordinal >= attrData.ConstructorArguments.Length)
                {
                    break;
                }

                typedConstant = attrData.ConstructorArguments[ordinal];

                if (argumentSyntaxes.Count <= ordinal)
                {
                    break;
                }

                syntax = argumentSyntaxes[ordinal];
                return true;
            }
        }

        foreach (var argument in attrData.NamedArguments)
        {
            if (string.Equals(argument.Key, propertyName, StringComparison.Ordinal))
            {
                typedConstant = argument.Value;
                syntax = argumentSyntaxes.FirstOrDefault(x =>
                    string.Equals(x.NameEquals?.Name.Identifier.ValueText, propertyName, StringComparison.Ordinal)
                );

                return syntax is not null;
            }
        }

        return false;
    }

    private static TypedConstant? GetTypedConstant(AttributeData attrData, string propertyName)
    {
        var constructorParameters = attrData.AttributeConstructor?.Parameters;
        if (constructorParameters is not null)
        {
            foreach (var parameter in constructorParameters)
            {
                if (!string.Equals(parameter.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var ordinal = parameter.Ordinal;
                return ordinal < attrData.ConstructorArguments.Length ? attrData.ConstructorArguments[ordinal] : null;
            }
        }

        foreach (var argument in attrData.NamedArguments)
        {
            if (string.Equals(argument.Key, propertyName, StringComparison.Ordinal))
            {
                return argument.Value;
            }
        }

        return null;
    }

    private static AttributeValue? GetAttributeValue(AttributeData attrData, string propertyName) =>
        TryGetTypedConstantWithAttributeArgumentSyntax(attrData, propertyName, out var typedConstant, out var syntax)
            ? new AttributeValue(typedConstant.Value, syntax.Expression)
            : null;
}
