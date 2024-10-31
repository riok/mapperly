using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Symbols.Members;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders;

public static class NewValueTupleMappingBodyBuilder
{
    public static void BuildMappingBody(MappingBuilderContext ctx, NewValueTupleConstructorMapping expressionMapping)
    {
        var mappingCtx = new NewValueTupleConstructorBuilderContext<NewValueTupleConstructorMapping>(ctx, expressionMapping);
        BuildTupleConstructorMapping(mappingCtx);
        mappingCtx.AddDiagnostics(true);
    }

    public static void BuildMappingBody(MappingBuilderContext ctx, NewValueTupleExpressionMapping expressionMapping)
    {
        var mappingCtx = new NewValueTupleExpressionBuilderContext<NewValueTupleExpressionMapping>(ctx, expressionMapping);
        BuildTupleConstructorMapping(mappingCtx);
        ObjectMemberMappingBodyBuilder.BuildMappingBody(mappingCtx);
        mappingCtx.AddDiagnostics(true);
    }

    private static void BuildTupleConstructorMapping(INewValueTupleBuilderContext<INewValueTupleMapping> ctx)
    {
        Debug.Assert(ctx.Mapping.TargetType.IsTupleType);
        Debug.Assert(ctx.Mapping.TargetType is INamedTypeSymbol);

        if (!TryBuildTupleConstructorMapping(ctx, out var constructorParameterMappings))
        {
            ctx.BuilderContext.ReportDiagnostic(DiagnosticDescriptors.NoConstructorFound, ctx.BuilderContext.Target);
            return;
        }

        foreach (var mapping in constructorParameterMappings)
        {
            ctx.AddTupleConstructorParameterMapping(mapping);
        }
    }

    private static bool TryBuildTupleConstructorMapping(
        INewValueTupleBuilderContext<INewValueTupleMapping> ctx,
        out List<ValueTupleConstructorParameterMapping> constructorParameterMappings
    )
    {
        constructorParameterMappings = [];

        var targetMembers = ctx.EnumerateUnmappedTargetMembers().ToList();

        // this can only happen if a target member is ignored
        // if this is the case, a mapping can never be created...
        if (targetMembers.Count != ((INamedTypeSymbol)ctx.Mapping.TargetType).TupleElements.Length)
            return false;

        foreach (var targetMember in targetMembers)
        {
            var targetField = ((FieldMember)targetMember).Symbol;
            if (!ctx.TryMatchTupleElement(targetField, out var memberMappingInfo))
            {
                ctx.BuilderContext.ReportDiagnostic(
                    DiagnosticDescriptors.SourceMemberNotFound,
                    targetMember.Name,
                    ctx.Mapping.TargetType,
                    ctx.Mapping.SourceType
                );
                ctx.SetTargetMemberMapped(targetMember);
                return false;
            }

            if (!SourceValueBuilder.TryBuildMappedSourceValue(ctx, memberMappingInfo, out var mappedSourceValue))
            {
                ctx.SetTargetMemberMapped(targetMember);
                return false;
            }

            var ctorMapping = new ValueTupleConstructorParameterMapping(targetField, mappedSourceValue, memberMappingInfo);
            constructorParameterMappings.Add(ctorMapping);
        }

        return true;
    }
}
