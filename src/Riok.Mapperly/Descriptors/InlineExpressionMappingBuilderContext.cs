using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
using Riok.Mapperly.Descriptors.Mappings.UserMappings;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors;

/// <summary>
/// A <see cref="MappingBuilderContext"/> implementation,
/// which tries to only build mappings which are safe to be used in <see cref="System.Linq.Expressions.Expression{TDelegate}"/>.
/// </summary>
public class InlineExpressionMappingBuilderContext : MappingBuilderContext
{
    private readonly MappingCollection _inlineExpressionMappings;
    private readonly MappingBuilderContext _parentContext;

    public InlineExpressionMappingBuilderContext(MappingBuilderContext ctx, ITypeSymbol sourceType, ITypeSymbol targetType)
        : this(ctx, (ctx.FindMapping(sourceType, targetType) as IUserMapping)?.Method, sourceType, targetType) { }

    private InlineExpressionMappingBuilderContext(
        MappingBuilderContext ctx,
        IMethodSymbol? userSymbol,
        ITypeSymbol source,
        ITypeSymbol target
    )
        : base(ctx, userSymbol, source, target, false)
    {
        _parentContext = ctx;
        _inlineExpressionMappings = new MappingCollection();
    }

    private InlineExpressionMappingBuilderContext(
        InlineExpressionMappingBuilderContext ctx,
        IMethodSymbol? userSymbol,
        ITypeSymbol source,
        ITypeSymbol target,
        bool clearDerivedTypes
    )
        : base(ctx, userSymbol, source, target, clearDerivedTypes)
    {
        _parentContext = ctx;
        _inlineExpressionMappings = ctx._inlineExpressionMappings;
    }

    public override bool IsExpression => true;

    public override bool IsConversionEnabled(MappingConversionType conversionType)
        // cannot convert enum to string via optimized mapping since this would include a switch
        // expression, which is not valid in an expression.
        // fall back to the ToString implementation.
        =>
        conversionType is not MappingConversionType.EnumToString and not MappingConversionType.Dictionary
        && base.IsConversionEnabled(conversionType);

    /// <summary>
    /// Tries to find an existing mapping for the provided types.
    /// The nullable annotation of reference types is ignored and always set to non-nullable.
    /// Only inline expression mappings and user implemented mappings are considered.
    /// </summary>
    /// <param name="sourceType">The source type.</param>
    /// <param name="targetType">The target type.</param>
    /// <returns>The <see cref="INewInstanceMapping"/> if a mapping was found or <c>null</c> if none was found.</returns>
    public override INewInstanceMapping? FindMapping(ITypeSymbol sourceType, ITypeSymbol targetType)
    {
        if (_inlineExpressionMappings.Find(sourceType, targetType) is { } mapping)
            return mapping;

        // User implemented mappings are also taken into account.
        // This works as long as the user implemented methods
        // follow the expression tree limitations:
        // https://learn.microsoft.com/en-us/dotnet/csharp/advanced-topics/expression-trees/#limitations
        if (_parentContext.FindMapping(sourceType, targetType) is UserImplementedMethodMapping userMapping)
        {
            _inlineExpressionMappings.Add(userMapping);
            return userMapping;
        }

        return null;
    }

    /// <summary>
    /// Always builds a new mapping with the user symbol of the first user defined mapping method for the provided types
    /// or no user symbol if no user defined mapping is available unless if this <see cref="InlineExpressionMappingBuilderContext"/>
    /// already built a mapping for the specified types, then this mapping is reused.
    /// The nullable annotation of reference types is ignored and always set to non-nullable.
    /// This ensures, the configuration of the user defined method is reused.
    /// <seealso cref="MappingBuilderContext.FindOrBuildMapping"/>
    /// </summary>
    /// <param name="sourceType">The source type.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="options">The options, <see cref="MappingBuildingOptions.MarkAsReusable"/> is ignored.</param>
    /// <returns></returns>
    public override INewInstanceMapping? FindOrBuildMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        MappingBuildingOptions options = MappingBuildingOptions.Default
    )
    {
        sourceType = sourceType.UpgradeNullable();
        targetType = targetType.UpgradeNullable();
        var mapping = FindMapping(sourceType, targetType);
        if (mapping != null)
            return mapping;

        var userSymbol = options.HasFlag(MappingBuildingOptions.KeepUserSymbol) ? UserSymbol : null;

        userSymbol ??= (MappingBuilder.Find(sourceType, targetType) as IUserMapping)?.Method;

        // unset MarkAsReusable and KeepUserSymbol as they have special handling for inline mappings
        options &= ~(MappingBuildingOptions.MarkAsReusable | MappingBuildingOptions.KeepUserSymbol);

        mapping = BuildMapping(userSymbol, sourceType, targetType, options);
        if (mapping != null)
        {
            _inlineExpressionMappings.Add(mapping);
        }

        return mapping;
    }

    /// <summary>
    /// Existing target instance mappings are not supported.
    /// </summary>
    /// <param name="sourceType">The source type, ignored.</param>
    /// <param name="targetType">The target type, ignored.</param>
    /// <param name="options">The options to build a new mapping, ignored.</param>
    /// <returns><c>null</c></returns>
    public override IExistingTargetMapping? FindOrBuildExistingTargetMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        MappingBuildingOptions options = MappingBuildingOptions.Default
    ) => null;

    /// <summary>
    /// Existing target instance mappings are not supported.
    /// </summary>
    /// <param name="sourceType">The source type, ignored.</param>
    /// <param name="targetType">The target type, ignored.</param>
    /// <param name="options">The options to build a new mapping, ignored.</param>
    /// <returns><c>null</c></returns>
    public override IExistingTargetMapping? BuildExistingTargetMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        MappingBuildingOptions options = MappingBuildingOptions.Default
    ) => null;

    protected override NullFallbackValue GetNullFallbackValue(ITypeSymbol targetType, bool throwOnMappingNullMismatch) =>
        base.GetNullFallbackValue(targetType, false); // never throw inside expressions (not translatable)

    protected override MappingBuilderContext ContextForMapping(
        IMethodSymbol? userSymbol,
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        MappingBuildingOptions options
    ) =>
        new InlineExpressionMappingBuilderContext(
            this,
            userSymbol,
            sourceType,
            targetType,
            options.HasFlag(MappingBuildingOptions.ClearDerivedTypes)
        );
}
