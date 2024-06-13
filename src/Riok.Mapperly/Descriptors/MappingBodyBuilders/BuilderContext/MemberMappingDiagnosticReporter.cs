using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;

internal static class MemberMappingDiagnosticReporter
{
    public static void ReportDiagnostics(MappingBuilderContext ctx, MembersMappingState state)
    {
        AddUnusedTargetMembersDiagnostics(ctx, state);
        AddUnmappedSourceMembersDiagnostics(ctx, state);
        AddUnmappedTargetMembersDiagnostics(ctx, state);
        AddNoMemberMappedDiagnostic(ctx, state);
    }

    private static void AddUnusedTargetMembersDiagnostics(MappingBuilderContext ctx, MembersMappingState state)
    {
        foreach (var memberConfig in state.UnusedMemberConfigs)
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.ConfiguredMappingTargetMemberNotFound, memberConfig.Target.FullName, ctx.Target);
        }
    }

    private static void AddUnmappedSourceMembersDiagnostics(MappingBuilderContext ctx, MembersMappingState state)
    {
        if (!ctx.Configuration.Members.RequiredMappingStrategy.HasFlag(RequiredMappingStrategy.Source))
            return;

        foreach (var sourceMemberName in state.UnmappedSourceMemberNames)
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.SourceMemberNotMapped, sourceMemberName, ctx.Source, ctx.Target);
        }
    }

    private static void AddUnmappedTargetMembersDiagnostics(MappingBuilderContext ctx, MembersMappingState state)
    {
        foreach (var targetMember in state.EnumerateUnmappedTargetMembers())
        {
            if (targetMember.IsRequired)
            {
                ctx.ReportDiagnostic(DiagnosticDescriptors.RequiredMemberNotMapped, targetMember.Name, ctx.Target, ctx.Source);
                continue;
            }

            if (targetMember.CanSet && ctx.Configuration.Members.RequiredMappingStrategy.HasFlag(RequiredMappingStrategy.Target))
            {
                ctx.ReportDiagnostic(DiagnosticDescriptors.SourceMemberNotFound, targetMember.Name, ctx.Target, ctx.Source);
            }
        }
    }

    private static void AddNoMemberMappedDiagnostic(MappingBuilderContext ctx, MembersMappingState state)
    {
        if (!state.HasMemberMapping)
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.NoMemberMappings, ctx.Source.ToDisplayString(), ctx.Target.ToDisplayString());
        }
    }
}
