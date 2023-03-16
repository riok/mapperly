using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors;

/// <summary>
/// A <see cref="MappingBuilderContext"/> implementation,
/// which tries to only build mappings which are safe to be used in <see cref="System.Linq.Expressions.Expression{TDelegate}"/>.
/// </summary>
public class InlineExpressionMappingBuilderContext : MappingBuilderContext
{
    private readonly MappingCollection _inlineExpressionMappings;

    public InlineExpressionMappingBuilderContext(MappingBuilderContext ctx, ITypeSymbol sourceType, ITypeSymbol targetType)
        : this(ctx, (ctx.FindMapping(sourceType, targetType) as IUserMapping)?.Method, sourceType, targetType)
    {
    }

    private InlineExpressionMappingBuilderContext(
        MappingBuilderContext ctx,
        IMethodSymbol? userSymbol,
        ITypeSymbol source,
        ITypeSymbol target)
        : base(ctx, userSymbol, source, target)
    {
        _inlineExpressionMappings = new MappingCollection();
    }

    private InlineExpressionMappingBuilderContext(
        InlineExpressionMappingBuilderContext ctx,
        IMethodSymbol? userSymbol,
        ITypeSymbol source,
        ITypeSymbol target)
        : base(ctx, userSymbol, source, target)
    {
        _inlineExpressionMappings = ctx._inlineExpressionMappings;
    }

    public override bool IsExpression => true;

    public override bool IsConversionEnabled(MappingConversionType conversionType)
        // cannot convert enum to string via optimized mapping since this would include a switch
        // expression, which is not valid in an expression.
        // fall back to the ToString implementation.
        => conversionType
                is not MappingConversionType.EnumToString
                and not MappingConversionType.Dictionary
            && base.IsConversionEnabled(conversionType);

    /// <summary>
    /// Tries to find an existing mapping for the provided types.
    /// The nullable annotation of reference types is ignored and always set to non-nullable.
    /// </summary>
    /// <param name="sourceType">The source type.</param>
    /// <param name="targetType">The target type.</param>
    /// <returns>The <see cref="ITypeMapping"/> if a mapping was found or <c>null</c> if none was found.</returns>
    public override ITypeMapping? FindMapping(ITypeSymbol sourceType, ITypeSymbol targetType)
        => _inlineExpressionMappings.Find(sourceType, targetType);

    /// <summary>
    /// Existing target instance mappings are not supported.
    /// </summary>
    /// <param name="sourceType">The source type.</param>
    /// <param name="targetType">The target type.</param>
    /// <returns><c>null</c></returns>
    public override IExistingTargetMapping? FindOrBuildExistingTargetMapping(ITypeSymbol sourceType, ITypeSymbol targetType)
        => null;

    /// <summary>
    /// Existing target instance mappings are not supported.
    /// </summary>
    /// <param name="sourceType">The source type.</param>
    /// <param name="targetType">The target type.</param>
    /// <returns><c>null</c></returns>
    public override IExistingTargetMapping? BuildExistingTargetMappingWithUserSymbol(ITypeSymbol sourceType, ITypeSymbol targetType)
        => null;

    /// <summary>
    /// Always builds a new mapping with the user symbol of the first user defined mapping method for the provided types
    /// or no user symbol if no user defined mapping is available unless if this <see cref="InlineExpressionMappingBuilderContext"/>
    /// already built a mapping for the specified types, then this mapping is reused.
    /// The nullable annotation of reference types is ignored and always set to non-nullable.
    /// This ensures, the configuration of the user defined method is reused.
    /// <seealso cref="MappingBuilderContext.FindOrBuildMapping"/>
    /// </summary>
    /// <param name="userSymbol">The user symbol.</param>
    /// <param name="sourceType">The source type.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="reusable">Whether the built mapping is usable by other mappings, this implementation always sets this to false.</param>
    /// <returns></returns>
    protected override ITypeMapping? FindOrBuildMapping(IMethodSymbol? userSymbol, ITypeSymbol sourceType, ITypeSymbol targetType, bool reusable)
    {
        sourceType = sourceType.UpgradeNullable();
        targetType = targetType.UpgradeNullable();
        var mapping = _inlineExpressionMappings.Find(sourceType, targetType);
        if (mapping != null)
            return mapping;

        userSymbol ??= (MappingBuilder.Find(sourceType, targetType) as IUserMapping)?.Method;

        mapping = BuildMapping(userSymbol, sourceType, targetType, false);
        if (mapping != null)
        {
            _inlineExpressionMappings.Add(mapping);
        }

        return mapping;
    }

    protected override NullFallbackValue GetNullFallbackValue(ITypeSymbol targetType, bool throwOnMappingNullMismatch)
        => base.GetNullFallbackValue(targetType, false); // never throw inside expressions (not translatable)

    protected override MappingBuilderContext ContextForMapping(IMethodSymbol? userSymbol, ITypeSymbol sourceType, ITypeSymbol targetType)
        => new InlineExpressionMappingBuilderContext(this, userSymbol, sourceType, targetType);
}
