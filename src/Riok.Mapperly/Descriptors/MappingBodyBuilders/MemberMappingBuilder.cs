using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings.SourceValue;
using Riok.Mapperly.Descriptors.Mappings.UserMappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols.Members;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders;

/// <summary>
/// Builder for member mappings (member of objects).
/// </summary>
internal static class MemberMappingBuilder
{
    public enum CodeStyle
    {
        Expression,
        Statement,
    }

    public static bool TryBuildContainerAssignment(
        IMembersContainerBuilderContext<IMemberAssignmentTypeMapping> ctx,
        MemberMappingInfo memberInfo,
        out bool requiresNullHandling,
        [NotNullWhen(true)] out MemberAssignmentMapping? mapping
    )
    {
        if (!SourceValueBuilder.TryBuildMappedValue(ctx, memberInfo, CodeStyle.Statement, out var result))
        {
            mapping = null;
            requiresNullHandling = false;
            return false;
        }

        var targetPathSetter = memberInfo.TargetMember.BuildSetter(ctx.BuilderContext);
        requiresNullHandling = result.SourceValue is MappedMemberSourceValue { RequiresSourceNullCheck: true };
        mapping = new MemberAssignmentMapping(targetPathSetter, result.SourceValue, memberInfo, result.TargetOriginalValueGetter);
        return true;
    }

    public static bool TryBuildAssignment(
        IMembersBuilderContext<IMapping> ctx,
        MemberMappingInfo memberInfo,
        [NotNullWhen(true)] out MemberAssignmentMapping? mapping
    )
    {
        if (!SourceValueBuilder.TryBuildMappedValue(ctx, memberInfo, CodeStyle.Expression, out var result))
        {
            mapping = null;
            return false;
        }

        var targetMemberSetter = memberInfo.TargetMember.BuildSetter(ctx.BuilderContext);
        mapping = new MemberAssignmentMapping(targetMemberSetter, result.SourceValue, memberInfo, result.TargetOriginalValueGetter);
        return true;
    }

    public static bool TryBuild(
        IMembersBuilderContext<IMapping> ctx,
        MemberMappingInfo memberMappingInfo,
        CodeStyle codeStyle,
        [NotNullWhen(true)] out SourceValueBuilder.MappedValue? result
    )
    {
        var mappingKey = memberMappingInfo.ToTypeMappingKey();
        var delegateMapping = ctx.BuilderContext.FindOrBuildLooseNullableMapping(
            mappingKey,
            diagnosticLocation: memberMappingInfo.Configuration?.Location
        );

        var sourceMember = memberMappingInfo.SourceMember!;
        var targetMember = memberMappingInfo.TargetMember;

        if (delegateMapping == null)
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.CouldNotMapMember,
                sourceMember.MemberPath.ToDisplayString(),
                targetMember.ToDisplayString()
            );
            result = null;
            return false;
        }

        var memberTargetAcceptsNull = memberMappingInfo.TargetMember.Member.IsWriteNullable;
        var delegateTargetNullable = delegateMapping.TargetType.IsNullable();
        var memberSourceNullable = memberMappingInfo.IsSourceNullable;
        var delegateSourceNullable = delegateMapping.SourceType.IsNullable();

        if (
            memberMappingInfo.Configuration?.SuppressNullMismatchDiagnostic != true
            && memberSourceNullable
            && !memberTargetAcceptsNull
            && !(delegateSourceNullable && !delegateTargetNullable)
        )
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.NullableSourceValueToNonNullableTargetValue,
                sourceMember.MemberPath.FullName,
                sourceMember.MemberPath.RootType.ToDisplayString(),
                targetMember.FullName,
                targetMember.RootType.ToDisplayString()
            );
        }

        var targetValueGetter = GetTargetValueGetterIfNeeded(ctx, delegateMapping, memberMappingInfo.TargetMember);

        if (
            (memberSourceNullable == delegateSourceNullable && memberTargetAcceptsNull == delegateTargetNullable)
            || (memberSourceNullable && !memberTargetAcceptsNull && delegateSourceNullable && !delegateTargetNullable)
        )
        {
            var sourceValue = new MappedMemberSourceValue(
                delegateMapping,
                sourceMember.MemberPath.BuildGetter(ctx.BuilderContext),
                true,
                false
            );
            result = new SourceValueBuilder.MappedValue(sourceValue, targetValueGetter);
            return true;
        }

        if (codeStyle == CodeStyle.Statement)
        {
            result = new SourceValueBuilder.MappedValue(
                BuildBlockNullHandlingMapping(ctx, delegateMapping, sourceMember.MemberPath, targetMember),
                targetValueGetter
            );
            return true;
        }

        if (!ValidateLoopMapping(ctx, delegateMapping, sourceMember.MemberPath, targetMember))
        {
            result = null;
            return false;
        }

        result = new SourceValueBuilder.MappedValue(
            BuildInlineNullHandlingMapping(ctx, delegateMapping, sourceMember.MemberPath, targetMember.MemberWriteType),
            targetValueGetter
        );
        return true;
    }

    private static MemberPathGetter? GetTargetValueGetterIfNeeded(
        IMembersBuilderContext<IMapping> ctx,
        INewInstanceMapping delegateMapping,
        NonEmptyMemberPath targetMember
    ) =>
        delegateMapping is UserImplementedMethodMapping { TargetOriginalValueParameter: not null }
            ? targetMember.BuildGetter(ctx.BuilderContext)
            : null;

    private static bool ValidateLoopMapping(
        IMembersBuilderContext<IMapping> ctx,
        INewInstanceMapping delegateMapping,
        MemberPath sourceMember,
        NonEmptyMemberPath targetMember
    )
    {
        if (!ReferenceEqualityComparer.Instance.Equals(delegateMapping, ctx.Mapping))
            return true;

        if (targetMember.Member is ConstructorParameterMember)
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.ReferenceLoopInCtorMapping,
                sourceMember.ToDisplayString(includeMemberType: false),
                ctx.Mapping.TargetType,
                targetMember.ToDisplayString(includeRootType: false, includeMemberType: false)
            );
        }
        else
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.ReferenceLoopInInitOnlyMapping,
                sourceMember.ToDisplayString(includeMemberType: false),
                targetMember.ToDisplayString(includeMemberType: false)
            );
        }
        return false;
    }

    private static NullMappedMemberSourceValue BuildInlineNullHandlingMapping(
        IMembersBuilderContext<IMapping> ctx,
        INewInstanceMapping delegateMapping,
        MemberPath sourcePath,
        ITypeSymbol targetMemberType
    )
    {
        var nullFallback = NullFallbackValue.Default;
        if (!delegateMapping.SourceType.IsNullable() && sourcePath.IsAnyReadNullable())
        {
            nullFallback = ctx.BuilderContext.GetNullFallbackValue(targetMemberType);
        }

        return new NullMappedMemberSourceValue(
            delegateMapping,
            sourcePath.BuildGetter(ctx.BuilderContext),
            targetMemberType,
            nullFallback,
            !ctx.BuilderContext.IsExpression
        );
    }

    private static ISourceValue BuildBlockNullHandlingMapping(
        IMembersBuilderContext<IMapping> ctx,
        INewInstanceMapping delegateMapping,
        MemberPath sourceMember,
        NonEmptyMemberPath targetMember
    )
    {
        var sourceGetter = sourceMember.BuildGetter(ctx.BuilderContext);

        // no member of the source path is nullable, no null handling needed
        if (!sourceMember.IsAnyReadNullable())
        {
            return new MappedMemberSourceValue(delegateMapping, sourceGetter, false, true);
        }

        // If null property assignments are allowed,
        // and the delegate mapping accepts nullable types (and converts it to a non-nullable type),
        // or the mapping is synthetic and the target accepts nulls
        // access the source in a null save matter (via ?.) but no other special handling required.
        if (
            ctx.BuilderContext.Configuration.Mapper.AllowNullPropertyAssignment
            && (delegateMapping.SourceType.IsNullable() || delegateMapping.IsSynthetic && targetMember.Member.IsWriteNullable)
        )
        {
            return new MappedMemberSourceValue(delegateMapping, sourceGetter, true, false);
        }

        // additional null condition check
        // (only map if the source is not null, else may throw depending on settings)
        // via RequiresNullCheck
        return new MappedMemberSourceValue(delegateMapping, sourceGetter, false, true);
    }
}
