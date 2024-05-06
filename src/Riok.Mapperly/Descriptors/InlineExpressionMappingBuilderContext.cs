using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.MappingBuilders;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
using Riok.Mapperly.Descriptors.Mappings.UserMappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors;

/// <summary>
/// A <see cref="MappingBuilderContext"/> implementation,
/// which tries to only build mappings which are safe to be used in <see cref="System.Linq.Expressions.Expression{TDelegate}"/>.
/// </summary>
public class InlineExpressionMappingBuilderContext : MappingBuilderContext
{
    public InlineExpressionMappingBuilderContext(MappingBuilderContext ctx, TypeMappingKey mappingKey)
        : base(ctx, ctx.FindMapping(mappingKey) as IUserMapping, null, mappingKey, false) { }

    private InlineExpressionMappingBuilderContext(
        InlineExpressionMappingBuilderContext ctx,
        IUserMapping? userMapping,
        Location? diagnosticLocation,
        TypeMappingKey mappingKey,
        bool ignoreDerivedTypes
    )
        : base(ctx, userMapping, diagnosticLocation, mappingKey, ignoreDerivedTypes) { }

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
    /// Only inline expression mappings and user implemented mappings are considered.
    /// User implemented mappings are tried to be inlined.
    /// </summary>
    public override INewInstanceMapping? FindNamedMapping(string mappingName)
    {
        var mapping = InlinedMappings.FindNamed(mappingName, out var ambiguousName, out var isInlined);
        if (mapping == null)
        {
            // resolve named but not yet discovered mappings
            mapping = base.FindNamedMapping(mappingName);
            isInlined = false;

            if (mapping == null)
                return null;
        }
        else if (ambiguousName)
        {
            ReportDiagnostic(DiagnosticDescriptors.ReferencedMappingAmbiguous, mappingName);
        }

        if (isInlined)
            return mapping;

        mapping = TryInlineMapping(mapping);
        InlinedMappings.SetInlinedMapping(mappingName, mapping);
        return mapping;
    }

    /// <summary>
    /// Tries to find an existing mapping for the provided types + config.
    /// The nullable annotation of reference types is ignored and always set to non-nullable.
    /// Only inline expression mappings and user implemented mappings are considered.
    /// User implemented mappings are tried to be inlined.
    /// </summary>
    /// <param name="mappingKey">The mapping key.</param>
    /// <returns>The <see cref="INewInstanceMapping"/> if a mapping was found or <c>null</c> if none was found.</returns>
    public override INewInstanceMapping? FindMapping(TypeMappingKey mappingKey)
    {
        var mapping = InlinedMappings.Find(mappingKey, out var isInlined);
        if (mapping == null)
            return null;

        if (isInlined)
            return mapping;

        mapping = TryInlineMapping(mapping);
        InlinedMappings.SetInlinedMapping(mappingKey, mapping);
        return mapping;
    }

    public INewInstanceMapping? FindNewInstanceMapping(IMethodSymbol method)
    {
        INewInstanceMapping? mapping = InlinedMappings.FindNewInstanceUserMapping(method, out var isInlined);
        if (mapping == null)
            return null;

        if (isInlined)
            return mapping;

        mapping = TryInlineMapping(mapping);
        InlinedMappings.SetInlinedMapping(new TypeMappingKey(mapping), mapping);
        return mapping;
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
        var userMapping = options.HasFlag(MappingBuildingOptions.KeepUserSymbol) ? UserMapping : null;

        // inline expression mappings don't reuse the user-defined mappings directly
        // but to apply the same configurations the default mapping user symbol is used
        // if there is no other user symbol.
        // this makes sure the configuration of the default mapping user symbol is used
        // for inline expression mappings.
        // This is not needed for regular mappings as these user defined method mappings
        // are directly built (with KeepUserSymbol) and called by the other mappings.
        userMapping ??= (MappingBuilder.Find(mappingKey) as IUserMapping);
        options &= ~MappingBuildingOptions.KeepUserSymbol;
        return BuildMapping(userMapping, mappingKey, options, diagnosticLocation);
    }

    protected override INewInstanceMapping? BuildMapping(
        IUserMapping? userMapping,
        TypeMappingKey mappingKey,
        MappingBuildingOptions options,
        Location? diagnosticLocation
    )
    {
        // unset mark as reusable as an inline expression mapping
        // should never be reused by the default mapping builder context,
        // only by other inline mapping builder contexts.
        var reusable = options.HasFlag(MappingBuildingOptions.MarkAsReusable);
        options &= ~MappingBuildingOptions.MarkAsReusable;

        var mapping = base.BuildMapping(userMapping, mappingKey, options, diagnosticLocation);
        if (mapping == null)
            return null;

        if (reusable)
        {
            InlinedMappings.AddMapping(mapping, mappingKey.Configuration);
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
        IUserMapping? userMapping,
        TypeMappingKey mappingKey,
        MappingBuildingOptions options,
        Location? diagnosticLocation = null
    )
    {
        return new InlineExpressionMappingBuilderContext(
            this,
            userMapping,
            diagnosticLocation,
            mappingKey,
            options.HasFlag(MappingBuildingOptions.IgnoreDerivedTypes)
        );
    }

    private INewInstanceMapping TryInlineMapping(INewInstanceMapping mapping)
    {
        return mapping switch
        {
            // inline existing mapping
            UserImplementedMethodMapping implementedMapping
                => InlineExpressionMappingBuilder.TryBuildMapping(this, implementedMapping) ?? implementedMapping,

            // build an inlined version
            IUserMapping userMapping
                => BuildMapping(
                    userMapping,
                    new TypeMappingKey(userMapping),
                    MappingBuildingOptions.Default,
                    userMapping.Method.GetSyntaxLocation()
                ) ?? mapping,

            _ => mapping,
        };
    }
}
