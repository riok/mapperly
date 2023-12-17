using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Configuration;
using Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders;

public static class NewValueTupleMappingBodyBuilder
{
    public static void BuildMappingBody(MappingBuilderContext ctx, NewValueTupleConstructorMapping expressionMapping)
    {
        var mappingCtx = new NewValueTupleConstructorBuilderContext<NewValueTupleConstructorMapping>(ctx, expressionMapping);
        BuildTupleConstructorMapping(mappingCtx);
        mappingCtx.AddDiagnostics();
    }

    public static void BuildMappingBody(MappingBuilderContext ctx, NewValueTupleExpressionMapping expressionMapping)
    {
        var mappingCtx = new NewValueTupleExpressionBuilderContext<NewValueTupleExpressionMapping>(ctx, expressionMapping);
        BuildTupleConstructorMapping(mappingCtx);
        ObjectMemberMappingBodyBuilder.BuildMappingBody(mappingCtx);
        mappingCtx.AddDiagnostics();
    }

    private static void BuildTupleConstructorMapping(INewValueTupleBuilderContext<INewValueTupleMapping> ctx)
    {
        if (ctx.Mapping.TargetType is not INamedTypeSymbol namedTargetType)
        {
            ctx.BuilderContext.ReportDiagnostic(DiagnosticDescriptors.NoConstructorFound, ctx.BuilderContext.Target);
            return;
        }

        if (!TryBuildTupleConstructorMapping(ctx, namedTargetType, out var constructorParameterMappings, out var mappedTargetMemberNames))
        {
            ctx.BuilderContext.ReportDiagnostic(DiagnosticDescriptors.NoConstructorFound, ctx.BuilderContext.Target);
            return;
        }

        var removableMappedTargetMemberNames = mappedTargetMemberNames.Where(x => !ctx.MemberConfigsByRootTargetName.ContainsKey(x));

        ctx.TargetMembers.RemoveRange(removableMappedTargetMemberNames);
        foreach (var constructorParameterMapping in constructorParameterMappings)
        {
            ctx.AddTupleConstructorParameterMapping(constructorParameterMapping);
        }
    }

    private static bool TryBuildTupleConstructorMapping(
        INewValueTupleBuilderContext<INewValueTupleMapping> ctx,
        INamedTypeSymbol namedTargetType,
        out HashSet<ValueTupleConstructorParameterMapping> constructorParameterMappings,
        out HashSet<string> mappedTargetMemberNames
    )
    {
        mappedTargetMemberNames = new HashSet<string>();
        constructorParameterMappings = new HashSet<ValueTupleConstructorParameterMapping>();

        foreach (var targetMember in namedTargetType.TupleElements)
        {
            if (!ctx.TargetMembers.ContainsKey(targetMember.Name))
            {
                return false;
            }

            if (!TryFindConstructorParameterSourcePath(ctx, targetMember, out var sourcePath, out var memberConfig))
            {
                ctx.BuilderContext.ReportDiagnostic(
                    DiagnosticDescriptors.SourceMemberNotFound,
                    targetMember.Name,
                    ctx.Mapping.TargetType,
                    ctx.Mapping.SourceType
                );

                return false;
            }

            // nullability is handled inside the member expressionMapping
            var paramType = targetMember.Type.WithNullableAnnotation(targetMember.NullableAnnotation);
            var mappingKey = new TypeMappingKey(sourcePath.MemberType, paramType, memberConfig?.ToTypeMappingConfiguration());
            var delegateMapping = ctx.BuilderContext.FindOrBuildLooseNullableMapping(
                mappingKey,
                diagnosticLocation: memberConfig?.Location
            );
            if (delegateMapping == null)
            {
                ctx.BuilderContext.ReportDiagnostic(
                    DiagnosticDescriptors.CouldNotMapMember,
                    ctx.Mapping.SourceType,
                    sourcePath.FullName,
                    sourcePath.Member.Type,
                    ctx.Mapping.TargetType,
                    targetMember.Name,
                    targetMember.Type
                );
                return false;
            }

            if (delegateMapping.Equals(ctx.Mapping))
            {
                ctx.BuilderContext.ReportDiagnostic(
                    DiagnosticDescriptors.ReferenceLoopInCtorMapping,
                    ctx.Mapping.SourceType,
                    sourcePath.FullName,
                    ctx.Mapping.TargetType,
                    targetMember.Name
                );
                return false;
            }

            var getterSourcePath = GetterMemberPath.Build(ctx.BuilderContext, sourcePath);

            var memberMapping = new NullMemberMapping(
                delegateMapping,
                getterSourcePath,
                paramType,
                ctx.BuilderContext.GetNullFallbackValue(paramType),
                !ctx.BuilderContext.IsExpression
            );

            var ctorMapping = new ValueTupleConstructorParameterMapping(targetMember, memberMapping);
            constructorParameterMappings.Add(ctorMapping);
            mappedTargetMemberNames.Add(targetMember.Name);
        }

        return true;
    }

    private static bool TryFindConstructorParameterSourcePath(
        INewValueTupleBuilderContext<INewValueTupleMapping> ctx,
        IFieldSymbol field,
        [NotNullWhen(true)] out MemberPath? sourcePath,
        out PropertyMappingConfiguration? memberConfig
    )
    {
        sourcePath = null;
        memberConfig = null;

        if (!ctx.MemberConfigsByRootTargetName.TryGetValue(field.Name, out var memberConfigs))
            return TryBuildConstructorParameterSourcePath(ctx, field, out sourcePath);

        // remove nested targets
        var initMemberPaths = memberConfigs.Where(x => x.Target.Path.Count == 1).ToArray();

        // if all memberConfigs are nested than do normal mapping
        if (initMemberPaths.Length == 0)
            return TryBuildConstructorParameterSourcePath(ctx, field, out sourcePath);

        if (initMemberPaths.Length > 1)
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.MultipleConfigurationsForConstructorParameter,
                field.Type,
                field.Name
            );
        }

        memberConfig = initMemberPaths.First();
        if (ctx.BuilderContext.SymbolAccessor.TryFindMemberPath(ctx.Mapping.SourceType, memberConfig.Source.Path, out sourcePath))
            return true;

        ctx.BuilderContext.ReportDiagnostic(
            DiagnosticDescriptors.SourceMemberNotFound,
            memberConfig.Source,
            ctx.Mapping.TargetType,
            ctx.Mapping.SourceType
        );
        return false;
    }

    private static bool TryBuildConstructorParameterSourcePath(
        INewValueTupleBuilderContext<INewValueTupleMapping> ctx,
        IFieldSymbol field,
        out MemberPath? sourcePath
    )
    {
        var ignoreCase = ctx.BuilderContext.MapperConfiguration.PropertyNameMappingStrategy == PropertyNameMappingStrategy.CaseInsensitive;

        if (
            ctx.BuilderContext.SymbolAccessor.TryFindMemberPath(
                ctx.Mapping.SourceType,
                MemberPathCandidateBuilder.BuildMemberPathCandidates(field.Name),
                ctx.IgnoredSourceMemberNames,
                ignoreCase,
                out sourcePath
            )
        )
        {
            return true;
        }

        // if standard matching fails, try to use the positional fields
        // if source is a tuple compare the underlying field ie, Item1, Item2
        if (!ctx.Mapping.SourceType.IsTupleType || ctx.Mapping.SourceType is not INamedTypeSymbol namedType)
            return false;

        var mappableField = namedType.TupleElements.FirstOrDefault(
            x =>
                x.CorrespondingTupleField != default
                && !ctx.IgnoredSourceMemberNames.Contains(x.Name)
                && string.Equals(field.CorrespondingTupleField!.Name, x.CorrespondingTupleField!.Name)
        );

        if (mappableField == default)
            return false;

        sourcePath = new MemberPath(new[] { new FieldMember(mappableField) });
        return true;
    }
}
