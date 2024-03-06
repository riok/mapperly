using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
using Riok.Mapperly.Descriptors.Mappings.UserMappings;

namespace Riok.Mapperly.Descriptors;

/// <summary>
/// A <see cref="MappingBuilderContext"/> implementation,
/// which tries to only build mappings which are safe to be used in <see cref="System.Linq.Expressions.Expression{TDelegate}"/>.
/// </summary>
public class InlineExpressionMappingBuilderContext : MappingBuilderContext
{
    private readonly MappingCollection _inlineExpressionMappings;
    private readonly MappingBuilderContext _parentContext;

    public InlineExpressionMappingBuilderContext(MappingBuilderContext ctx, TypeMappingKey mappingKey)
        : base(ctx, (ctx.FindMapping(mappingKey) as IUserMapping)?.Method, null, mappingKey, false)
    {
        _parentContext = ctx;
        _inlineExpressionMappings = new MappingCollection();
    }

    private InlineExpressionMappingBuilderContext(
        InlineExpressionMappingBuilderContext ctx,
        IMethodSymbol? userSymbol,
        Location? diagnosticLocation,
        TypeMappingKey mappingKey,
        bool ignoreDerivedTypes
    )
        : base(ctx, userSymbol, diagnosticLocation, mappingKey, ignoreDerivedTypes)
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
    /// <inheritdoc cref="MappingBuilderContext.FindNamedMapping"/>
    /// Only returns <see cref="INewInstanceUserMapping"/>s.
    /// </summary>
    public override INewInstanceMapping? FindNamedMapping(string mappingName)
    {
        // Only user implemented mappings are taken into account.
        // This works as long as the user implemented methods
        // follow the expression tree limitations:
        // https://learn.microsoft.com/en-us/dotnet/csharp/advanced-topics/expression-trees/#limitations
        if (base.FindNamedMapping(mappingName) is INewInstanceUserMapping mapping)
            return mapping;

        return null;
    }

    /// <summary>
    /// Tries to find an existing mapping for the provided types + config.
    /// The nullable annotation of reference types is ignored and always set to non-nullable.
    /// Only inline expression mappings and user implemented mappings are considered.
    /// </summary>
    /// <param name="mappingKey">The mapping key.</param>
    /// <returns>The <see cref="INewInstanceMapping"/> if a mapping was found or <c>null</c> if none was found.</returns>
    public override INewInstanceMapping? FindMapping(TypeMappingKey mappingKey)
    {
        if (_inlineExpressionMappings.FindNewInstanceMapping(mappingKey) is { } mapping)
            return mapping;

        // User implemented mappings are also taken into account.
        // This works as long as the user implemented methods
        // follow the expression tree limitations:
        // https://learn.microsoft.com/en-us/dotnet/csharp/advanced-topics/expression-trees/#limitations
        if (_parentContext.FindMapping(mappingKey) is UserImplementedMethodMapping userMapping)
            return userMapping;

        return null;
    }

    /// <summary>
    /// Builds a new mapping but always uses the user symbol of the default defined mapping (if it is a user mapping)
    /// or no user symbol if no user default mapping exists.
    /// <seealso cref="MappingBuilderContext.BuildMapping"/>
    /// </summary>
    /// <param name="mappingKey">The mapping key.</param>
    /// <param name="options">The options, <see cref="MappingBuildingOptions.MarkAsReusable"/> is ignored.</param>
    /// <param name="diagnosticLocation">The updated to location where to report diagnostics if a new mapping is being built.</param>
    /// <returns>The created mapping, or <c>null</c> if no mapping could be created.</returns>
    public override INewInstanceMapping? BuildMapping(
        TypeMappingKey mappingKey,
        MappingBuildingOptions options = MappingBuildingOptions.Default,
        Location? diagnosticLocation = null
    )
    {
        var userSymbol = options.HasFlag(MappingBuildingOptions.KeepUserSymbol) ? UserSymbol : null;

        // inline expression mappings don't reuse the user-defined mappings directly
        // but to apply the same configurations the default mapping user symbol is used
        // if there is no other user symbol.
        // this makes sure the configuration of the default mapping user symbol is used
        // for inline expression mappings.
        // This is not needed for regular mappings as these user defined method mappings
        // are directly built (with KeepUserSymbol) and called by the other mappings.
        userSymbol ??= (MappingBuilder.Find(mappingKey) as IUserMapping)?.Method;
        options &= ~MappingBuildingOptions.KeepUserSymbol;
        return BuildMapping(userSymbol, mappingKey, options, diagnosticLocation);
    }

    protected override INewInstanceMapping? BuildMapping(
        IMethodSymbol? userSymbol,
        TypeMappingKey mappingKey,
        MappingBuildingOptions options,
        Location? diagnosticLocation
    )
    {
        // unset mark as reusable as an inline expression mapping
        // should never be reused by the default mapping builder context,
        // only by other inline mapping builder contexts.
        var reusable = (options & MappingBuildingOptions.MarkAsReusable) != MappingBuildingOptions.MarkAsReusable;
        options &= ~MappingBuildingOptions.MarkAsReusable;

        var mapping = base.BuildMapping(userSymbol, mappingKey, options, diagnosticLocation);
        if (reusable && mapping != null)
        {
            _inlineExpressionMappings.AddMapping(mapping, mappingKey.Configuration);
        }

        return mapping;
    }

    /// <summary>
    /// Existing target instance mappings are not supported.
    /// </summary>
    /// <param name="mappingKey">The mapping key, ignored.</param>
    /// <param name="options">The options to build a new mapping, ignored.</param>
    /// <returns><c>null</c></returns>
    public override IExistingTargetMapping? FindOrBuildExistingTargetMapping(
        TypeMappingKey mappingKey,
        MappingBuildingOptions options = MappingBuildingOptions.Default
    ) => null;

    /// <summary>
    /// Existing target instance mappings are not supported.
    /// </summary>
    /// <param name="mappingKey">The mapping key, ignored.</param>
    /// <param name="options">The options to build a new mapping, ignored.</param>
    /// <returns><c>null</c></returns>
    public override IExistingTargetMapping? BuildExistingTargetMapping(
        TypeMappingKey mappingKey,
        MappingBuildingOptions options = MappingBuildingOptions.Default
    ) => null;

    protected override NullFallbackValue GetNullFallbackValue(ITypeSymbol targetType, bool throwOnMappingNullMismatch) =>
        base.GetNullFallbackValue(targetType, false); // never throw inside expressions (not translatable)

    protected override MappingBuilderContext ContextForMapping(
        IMethodSymbol? userSymbol,
        TypeMappingKey mappingKey,
        MappingBuildingOptions options,
        Location? diagnosticLocation = null
    )
    {
        return new InlineExpressionMappingBuilderContext(
            this,
            userSymbol,
            diagnosticLocation,
            mappingKey,
            options.HasFlag(MappingBuildingOptions.IgnoreDerivedTypes)
        );
    }
}
