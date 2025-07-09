using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Tests;

public record TestSourceBuilderOptions(
    string? Namespace = null,
    string MapperClassName = TestSourceBuilderOptions.DefaultMapperClassName,
    string? MapperBaseClassName = null,
    bool? UseDeepCloning = null,
    bool? UseReferenceHandling = null,
    bool? ThrowOnMappingNullMismatch = null,
    bool? ThrowOnPropertyMappingNullMismatch = null,
    bool? AllowNullPropertyAssignment = null,
    PropertyNameMappingStrategy? PropertyNameMappingStrategy = null,
    MappingConversionType? EnabledConversions = null,
    EnumMappingStrategy? EnumMappingStrategy = null,
    bool? EnumMappingIgnoreCase = null,
    IgnoreObsoleteMembersStrategy? IgnoreObsoleteMembersStrategy = null,
    RequiredMappingStrategy? RequiredMappingStrategy = null,
    RequiredMappingStrategy? RequiredEnumMappingStrategy = null,
    MemberVisibility? IncludedMembers = null,
    MemberVisibility? IncludedConstructors = null,
    bool Static = false,
    bool PreferParameterlessConstructors = true,
    bool AutoUserMappings = true
)
{
    public const string DefaultMapperClassName = "Mapper";

    public static readonly TestSourceBuilderOptions Default = new();
    public static readonly TestSourceBuilderOptions AsStatic = new(Static: true);
    public static readonly TestSourceBuilderOptions WithDeepCloning = new(UseDeepCloning: true);
    public static readonly TestSourceBuilderOptions WithDeepCloningAndExplicitCast = new TestSourceBuilderOptions(
        UseDeepCloning: true,
        EnabledConversions: MappingConversionType.ExplicitCast
    );
    public static readonly TestSourceBuilderOptions WithDeepCloningDictionaryAndExplicitCast = new TestSourceBuilderOptions(
        UseDeepCloning: true,
        EnabledConversions: MappingConversionType.Dictionary | MappingConversionType.ExplicitCast
    );
    public static readonly TestSourceBuilderOptions WithDictionaryAndExplicitCast = new TestSourceBuilderOptions(
        EnabledConversions: MappingConversionType.Dictionary | MappingConversionType.ExplicitCast
    );
    public static readonly TestSourceBuilderOptions WithEnumUnderlyingTypeAndExplicitCast = new TestSourceBuilderOptions(
        EnabledConversions: MappingConversionType.EnumUnderlyingType | MappingConversionType.ExplicitCast
    );
    public static readonly TestSourceBuilderOptions WithEnumerableAndExplicitCast = new TestSourceBuilderOptions(
        EnabledConversions: MappingConversionType.Enumerable | MappingConversionType.ExplicitCast
    );
    public static readonly TestSourceBuilderOptions WithQueryableAndExplicitCast = new TestSourceBuilderOptions(
        EnabledConversions: MappingConversionType.Queryable | MappingConversionType.ExplicitCast
    );
    public static readonly TestSourceBuilderOptions WithQueryableAndEnumerableExplicitCast = new TestSourceBuilderOptions(
        EnabledConversions: MappingConversionType.Queryable | MappingConversionType.Enumerable | MappingConversionType.ExplicitCast
    );
    public static readonly TestSourceBuilderOptions WithExplicitCast = new TestSourceBuilderOptions(
        EnabledConversions: MappingConversionType.ExplicitCast
    );
    public static readonly TestSourceBuilderOptions WithReferenceHandling = new(UseReferenceHandling: true);
    public static readonly TestSourceBuilderOptions WithReferenceHandlingAndQueryableAndExplicitCast = new(
        UseReferenceHandling: true,
        EnabledConversions: MappingConversionType.Queryable | MappingConversionType.ExplicitCast
    );
    public static readonly TestSourceBuilderOptions WithDisabledAutoUserMappings = new(AutoUserMappings: false);

    public static readonly TestSourceBuilderOptions PreferParameterizedConstructors = new(PreferParameterlessConstructors: false);

    public static TestSourceBuilderOptions WithBaseClass(string baseClassName) => new(MapperBaseClassName: baseClassName);

    public static TestSourceBuilderOptions WithIgnoreObsolete(IgnoreObsoleteMembersStrategy ignoreObsoleteStrategy) =>
        new(IgnoreObsoleteMembersStrategy: ignoreObsoleteStrategy);

    public static TestSourceBuilderOptions WithRequiredMappingStrategy(RequiredMappingStrategy requiredMappingStrategy) =>
        new(RequiredMappingStrategy: requiredMappingStrategy);

    public static TestSourceBuilderOptions WithRequiredEnumMappingStrategy(
        RequiredMappingStrategy requiredEnumMappingStrategy,
        RequiredMappingStrategy requiredMappingStrategy
    ) => new(RequiredEnumMappingStrategy: requiredEnumMappingStrategy, RequiredMappingStrategy: requiredMappingStrategy);

    public static TestSourceBuilderOptions WithMemberVisibility(MemberVisibility memberVisibility) =>
        new(IncludedMembers: memberVisibility);

    public static TestSourceBuilderOptions WithConstructorVisibility(MemberVisibility memberVisibility) =>
        new(IncludedConstructors: memberVisibility);

    public static TestSourceBuilderOptions WithDisabledMappingConversion(params MappingConversionType[] conversionTypes)
    {
        var enabled = MappingConversionType.All;

        foreach (var disabledConversionType in conversionTypes)
        {
            enabled &= ~disabledConversionType;
        }

        return new(EnabledConversions: enabled);
    }
}
