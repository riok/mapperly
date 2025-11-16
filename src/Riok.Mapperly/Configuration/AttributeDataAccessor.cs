using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
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
public class AttributeDataAccessor(SymbolAccessor symbolAccessor) : IAttributeDataAccessor
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
        foreach (var attrData in GetAttributes<MapNestedPropertiesAttribute>(symbol))
        {
            var config = GetMemberPath(attrData, nameof(NestedMembersMappingConfiguration.Source));
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
        foreach (var attrData in GetAttributes<MapEnumValueAttribute>(symbol))
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
        foreach (var attrData in GetAttributes<MapperIgnoreSourceValueAttribute>(symbol))
        {
            var fieldSymbol = GetFieldSymbol(attrData, "source");
            if (fieldSymbol is null)
                continue;

            yield return new MapperIgnoreEnumValueConfiguration(fieldSymbol);
        }
    }

    public IEnumerable<MapperIgnoreEnumValueConfiguration> ReadMapperIgnoreTargetValueAttribute(ISymbol symbol)
    {
        foreach (var attrData in GetAttributes<MapperIgnoreTargetValueAttribute>(symbol))
        {
            var fieldSymbol = GetFieldSymbol(attrData, "target");
            if (fieldSymbol is null)
                continue;

            yield return new MapperIgnoreEnumValueConfiguration(fieldSymbol);
        }
    }

    public ComponentModelDescriptionAttributeConfiguration? ReadDescriptionAttribute(ISymbol symbol)
    {
        var attrData = GetAttribute<DescriptionAttribute>(symbol);
        if (attrData == null)
            return null;

        var simpleValue = GetSimpleValue(attrData, nameof(ComponentModelDescriptionAttributeConfiguration.Description));
        return new ComponentModelDescriptionAttributeConfiguration(simpleValue);
    }

    public UserMappingConfiguration? ReadUserMappingAttribute(ISymbol symbol)
    {
        var attrData = GetAttribute<UserMappingAttribute>(symbol);
        if (attrData == null)
            return null;

        return new UserMappingConfiguration
        {
            Default = GetSimpleValue<bool>(attrData, nameof(UserMappingAttribute.Default)),
            Ignore = GetSimpleValue<bool>(attrData, nameof(UserMappingAttribute.Ignore)),
        };
    }

    public bool HasUseMapperAttribute(ISymbol symbol)
    {
        return GetAttributes<UseMapperAttribute>(symbol).Any();
    }

    public IEnumerable<MapperIgnoreSourceAttribute> ReadMapperIgnoreSourceAttributes(ISymbol symbol)
    {
        foreach (var attrData in GetAttributes<MapperIgnoreSourceAttribute>(symbol))
        {
            var source = GetSimpleValue(attrData, nameof(MapperIgnoreSourceAttribute.Source));
            if (source is null)
                continue;
            yield return new MapperIgnoreSourceAttribute(source);
        }
    }

    public IEnumerable<MapperIgnoreTargetAttribute> ReadMapperIgnoreTargetAttributes(ISymbol symbol)
    {
        foreach (var attrData in GetAttributes<MapperIgnoreTargetAttribute>(symbol))
        {
            var target = GetSimpleValue(attrData, nameof(MapperIgnoreTargetAttribute.Target));
            if (target is null)
                continue;
            yield return new MapperIgnoreTargetAttribute(target);
        }
    }

    public IEnumerable<MemberValueMappingConfiguration> ReadMapValueAttribute(ISymbol symbol)
    {
        foreach (var attrData in GetAttributes<MapValueAttribute>(symbol))
        {
            var target = GetMemberPath(attrData, nameof(MapValueAttribute.Target));
            var value = GetAttributeValue(attrData, nameof(MapValueAttribute.Value));
            yield return new MemberValueMappingConfiguration(target, value)
            {
                Use = GetMethodReference(attrData, nameof(MapValueAttribute.Use)),
            };
        }
    }

    public IEnumerable<MemberMappingConfiguration> ReadMapPropertyAttributes(ISymbol symbol)
    {
        foreach (var attrData in GetAttributes<MapPropertyAttribute>(symbol))
        {
            var source = GetMemberPath(attrData, nameof(MapPropertyAttribute.Source));
            var target = GetMemberPath(attrData, nameof(MapPropertyAttribute.Target));
            var syntaxReference = attrData.ApplicationSyntaxReference?.GetSyntax();
            yield return new MemberMappingConfiguration(source, target)
            {
                FormatProvider = GetSimpleValue(attrData, nameof(MapPropertyAttribute.FormatProvider)),
                StringFormat = GetSimpleValue(attrData, nameof(MapPropertyAttribute.StringFormat)),
                SuppressNullMismatchDiagnostic =
                    GetSimpleValue<bool>(attrData, nameof(MapPropertyAttribute.SuppressNullMismatchDiagnostic)) ?? false,
                Use = GetMethodReference(attrData, nameof(MapPropertyAttribute.Use)),
                SyntaxReference = syntaxReference,
            };
        }
    }

    public IEnumerable<IncludeMappingConfiguration> ReadIncludeMappingConfigurationAttributes(ISymbol symbol)
    {
        foreach (var attrData in GetAttributes<IncludeMappingConfigurationAttribute>(symbol))
        {
            var name = GetMethodReference(attrData, nameof(IncludeMappingConfigurationAttribute.Name));
            if (name is null)
                continue;

            yield return new IncludeMappingConfiguration(name);
        }
    }

    public IEnumerable<DerivedTypeMappingConfiguration> ReadMapDerivedTypeAttributes(ISymbol symbol)
    {
        foreach (var attrData in GetAttributes<MapDerivedTypeAttribute>(symbol))
        {
            var sourceType = GetTypeSymbolFromValue(attrData, nameof(MapDerivedTypeAttribute.SourceType));
            var targetType = GetTypeSymbolFromValue(attrData, nameof(MapDerivedTypeAttribute.TargetType));

            if (sourceType is null || targetType is null)
                continue;

            yield return new DerivedTypeMappingConfiguration(sourceType, targetType);
        }
    }

    public IEnumerable<DerivedTypeMappingConfiguration> ReadGenericMapDerivedTypeAttributes(ISymbol symbol)
    {
        foreach (var attrData in GetAttributes<MapDerivedTypeAttribute<object, object>>(symbol))
        {
            var sourceType = GetTypeSymbolFromGenericArgument(attrData, 0);
            var targetType = GetTypeSymbolFromGenericArgument(attrData, 1);

            yield return new DerivedTypeMappingConfiguration(sourceType, targetType);
        }
    }

    public IEnumerable<MemberMappingConfiguration> ReadMapPropertyFromSourceAttributes(ISymbol symbol)
    {
        foreach (var attrData in GetAttributes<MapPropertyFromSourceAttribute>(symbol))
        {
            var target = GetMemberPath(attrData, nameof(MapPropertyFromSourceAttribute.Target));
            yield return new MemberMappingConfiguration(StringMemberPath.Empty, target)
            {
                StringFormat = GetSimpleValue(attrData, nameof(MapPropertyFromSourceAttribute.StringFormat)),
                FormatProvider = GetSimpleValue(attrData, nameof(MapPropertyFromSourceAttribute.FormatProvider)),
                Use = GetMethodReference(attrData, nameof(MapPropertyFromSourceAttribute.Use)),
                SyntaxReference = attrData.ApplicationSyntaxReference?.GetSyntax(),
            };
        }
    }

    public IEnumerable<UseStaticMapperConfiguration> ReadUseStaticMapperAttributes(ISymbol symbol)
    {
        foreach (var attrData in GetAttributes<UseStaticMapperAttribute>(symbol))
        {
            var type = GetTypeSymbolFromValue(attrData, "mapperType");
            if (type is null)
                continue;

            yield return new UseStaticMapperConfiguration(type);
        }
    }

    public IEnumerable<UseStaticMapperConfiguration> ReadGenericUseStaticMapperAttributes(ISymbol symbol)
    {
        foreach (var attrData in GetAttributes<UseStaticMapperAttribute<object>>(symbol))
        {
            var type = GetTypeSymbolFromGenericArgument(attrData, 0);
            yield return new UseStaticMapperConfiguration(type);
        }
    }

    public string GetMappingName(IMethodSymbol methodSymbol)
    {
        var attrData = GetAttribute<NamedMappingAttribute>(methodSymbol);
        if (attrData == null)
            return methodSymbol.Name;

        return GetSimpleValue(attrData, nameof(NamedMappingAttribute.Name)) ?? methodSymbol.Name;
    }

    public bool IsMappingNameEqualTo(IMethodSymbol methodSymbol, string name)
    {
        return string.Equals(GetMappingName(methodSymbol), name, StringComparison.Ordinal);
    }

    public IEnumerable<NotNullIfNotNullConfiguration> ReadNotNullIfNotNullAttributes(IMethodSymbol symbol)
    {
        foreach (var attrData in symbolAccessor.TryGetAttributes<NotNullIfNotNullAttribute>(symbol.GetReturnTypeAttributes()))
        {
            var parameterName = GetSimpleValue(attrData, nameof(NotNullIfNotNullAttribute.ParameterName));
            if (parameterName is null)
                continue;

            yield return new NotNullIfNotNullConfiguration(parameterName);
        }
    }

    private AttributeData? GetAttribute<TAttribute>(ISymbol symbol)
        where TAttribute : Attribute
    {
        return GetAttributes<TAttribute>(symbol).FirstOrDefault();
    }

    private AttributeData GetRequiredAttribute<TAttribute>(ISymbol symbol)
        where TAttribute : Attribute
    {
        return GetAttribute<TAttribute>(symbol)
            ?? throw new InvalidOperationException($"Could not find attribute {typeof(TAttribute).FullName} on {symbol}");
    }

    private IEnumerable<AttributeData> GetAttributes<TAttribute>(ISymbol symbol)
        where TAttribute : Attribute
    {
        return symbolAccessor.GetAttributes<TAttribute>(symbol);
    }

    private IMemberPathConfiguration GetMemberPath(AttributeData attrData, string name)
    {
        if (TryGetTypedConstant(attrData, name, out var typedConstant, out var argumentSyntax))
        {
            return CreateMemberPath(typedConstant, argumentSyntax);
        }

        return new StringMemberPath(name.Split(MemberPathConstants.MemberAccessSeparator).ToImmutableEquatableArray());
    }

    private IMethodReferenceConfiguration? GetMethodReference(AttributeData attrData, string name)
    {
        if (TryGetTypedConstant(attrData, name, out var typedConstant, out var argumentSyntax))
        {
            return CreateMethodReference(typedConstant, argumentSyntax);
        }

        return null;
    }

    private static string? GetSimpleValue(AttributeData attrData, string propertyName)
    {
        if (TryGetTypedConstant(attrData, propertyName, out var typedConstant, out _))
        {
            return typedConstant.Value as string;
        }

        return null;
    }

    private static TValue GetSimpleValueOrDefault<TValue>(AttributeData attrData, string propertyName, TValue defaultValue = default)
        where TValue : struct
    {
        return GetSimpleValue<TValue>(attrData, propertyName) ?? defaultValue;
    }

    private static IFieldSymbol? GetFieldSymbol(AttributeData attrData, string propertyName)
    {
        if (TryGetTypedConstant(attrData, propertyName, out var typedConstant, out _))
        {
            var roslynType = typedConstant.Type;
            return roslynType?.GetFields().FirstOrDefault(f => Equals(f.ConstantValue, typedConstant.Value));
        }

        return null;
    }

    private static ITypeSymbol? GetTypeSymbolFromValue(AttributeData attrData, string propertyName)
    {
        if (TryGetTypedConstant(attrData, propertyName, out var typedConstant, out _))
        {
            return typedConstant.Value as ITypeSymbol;
        }

        return null;
    }

    private static ITypeSymbol GetTypeSymbolFromGenericArgument(AttributeData attrData, int index)
    {
        return attrData.AttributeClass?.TypeArguments is { Length: > 0 } typeArguments && typeArguments.Length > index
            ? typeArguments[index]
            : throw new InvalidOperationException($"Could not get type argument {index} of attribute {attrData.AttributeClass?.Name}");
    }

    private static TValue? GetSimpleValue<TValue>(AttributeData attrData, string propertyName)
        where TValue : struct
    {
        if (TryGetTypedConstant(attrData, propertyName, out var typedConstant, out _))
        {
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
        }

        return null;
    }

    private static AttributeValue? GetAttributeValue(AttributeData attrData, string propertyName) =>
        TryGetTypedConstant(attrData, propertyName, out var typedConstant, out var syntax)
            ? new AttributeValue(typedConstant, syntax.Expression)
            : null;

    private static bool TryGetTypedConstant(
        AttributeData attrData,
        string propertyName,
        out TypedConstant typedConstant,
        [NotNullWhen(true)] out AttributeArgumentSyntax? syntax
    )
    {
        syntax = null;
        typedConstant = default;
        var syntaxes = (AttributeSyntax?)attrData.ApplicationSyntaxReference?.GetSyntax();
        if (syntaxes?.ArgumentList?.Arguments is not { } argumentSyntaxes)
        {
            return false;
        }

        if (attrData.AttributeConstructor?.Parameters is { } constructorParameters)
        {
            for (var index = 0; index < constructorParameters.Length; index++)
            {
                var parameter = constructorParameters[index];
                if (!string.Equals(parameter.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (index >= attrData.ConstructorArguments.Length)
                {
                    break;
                }

                typedConstant = attrData.ConstructorArguments[index];

                if (argumentSyntaxes.Count <= index)
                {
                    break;
                }

                syntax = argumentSyntaxes[index];
                return true;
            }
        }

        var argumentSyntaxStartIndex = attrData.AttributeConstructor?.Parameters.Length ?? 0;
        for (var index = 0; index < attrData.NamedArguments.Length; index++)
        {
            var argument = attrData.NamedArguments[index];
            if (!string.Equals(argument.Key, propertyName, StringComparison.Ordinal))
            {
                continue;
            }

            typedConstant = argument.Value;
            syntax = argumentSyntaxes[argumentSyntaxStartIndex + index];

            return true;
        }

        return false;
    }

    private IMemberPathConfiguration CreateMemberPath(TypedConstant arg, AttributeArgumentSyntax? syntax)
    {
        if (arg.Kind == TypedConstantKind.Array)
        {
            var values = arg.Values.Select(x => (string?)x.Value).WhereNotNull().ToImmutableEquatableArray();
            return new StringMemberPath(values);
        }

        if (arg.Kind == TypedConstantKind.Primitive && syntax.TryGetNameOfSyntax(out var invocationExpressionSyntax))
        {
            return CreateNameOfMemberPath(invocationExpressionSyntax);
        }

        if (arg is { Kind: TypedConstantKind.Primitive, Value: string v })
        {
            return new StringMemberPath(v.Split(MemberPathConstants.MemberAccessSeparator).ToImmutableEquatableArray());
        }

        throw new InvalidOperationException($"Cannot create {nameof(StringMemberPath)} from {arg.Kind}");
    }

    private IMemberPathConfiguration CreateNameOfMemberPath(InvocationExpressionSyntax nameofSyntax)
    {
        // @ prefix opts-in to full-nameof
        var fullNameOf = nameofSyntax.IsFullNameOfSyntax();

        var nameOfOperation = symbolAccessor.GetOperation<INameOfOperation>(nameofSyntax);
        var memberRefOperation = nameOfOperation?.GetFirstChildOperation<IMemberReferenceOperation>();
        if (memberRefOperation == null)
        {
            // fall back to the old skip-first-segment approach
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

    private IMethodReferenceConfiguration CreateMethodReference(TypedConstant arg, AttributeArgumentSyntax? syntax)
    {
        if (arg.Kind != TypedConstantKind.Primitive)
        {
            throw new InvalidOperationException($"Cannot create {nameof(IMethodReferenceConfiguration)} from {arg.Kind}");
        }

        if (
            syntax.TryGetNameOfSyntax(out var invocationExpressionSyntax)
            && invocationExpressionSyntax.IsFullNameOfSyntax()
            && TryCreateNameOfMethodReferenceConfiguration(invocationExpressionSyntax, out var configuration)
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

    private bool TryCreateNameOfMethodReferenceConfiguration(
        InvocationExpressionSyntax nameofSyntax,
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
}
