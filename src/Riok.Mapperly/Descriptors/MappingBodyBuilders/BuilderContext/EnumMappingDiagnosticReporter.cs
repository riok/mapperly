using Microsoft.CodeAnalysis;
using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;

internal static class EnumMappingDiagnosticReporter
{
    public static void AddUnmatchedSourceIgnoredMembers(MappingBuilderContext ctx, ISet<IFieldSymbol> ignoredSourceMembers)
    {
        var sourceFields = ctx.SymbolAccessor.GetAllFields(ctx.Source).ToHashSet();
        var unmatchedSourceMembers = ignoredSourceMembers.Where(m => !sourceFields.Contains(m));

        foreach (var member in unmatchedSourceMembers)
        {
            ctx.ReportDiagnostic(
                DiagnosticDescriptors.IgnoredEnumSourceMemberNotFound,
                member.Name,
                member.ConstantValue!,
                ctx.Source,
                ctx.Target
            );
        }
    }

    public static void AddUnmatchedTargetIgnoredMembers(MappingBuilderContext ctx, ISet<IFieldSymbol> ignoredTargetMembers)
    {
        var targetFields = ctx.SymbolAccessor.GetAllFields(ctx.Target).ToHashSet();
        var unmatchedTargetMembers = ignoredTargetMembers.Where(m => !targetFields.Contains(m));

        foreach (var member in unmatchedTargetMembers)
        {
            ctx.ReportDiagnostic(
                DiagnosticDescriptors.IgnoredEnumTargetMemberNotFound,
                member.Name,
                member.ConstantValue!,
                ctx.Source,
                ctx.Target
            );
        }
    }

    public static void AddUnmappedTargetMembersDiagnostics(
        MappingBuilderContext ctx,
        ISet<IFieldSymbol> mappings,
        IEnumerable<IFieldSymbol> targetMembers
    )
    {
        var fallbackValue = ctx.Configuration.Enum.FallbackValue?.ConstantValue.Value;
        var missingTargetMembers = targetMembers.Where(field =>
            !mappings.Contains(field) && fallbackValue?.Equals(field.ConstantValue) is not true
        );
        foreach (var member in missingTargetMembers)
        {
            ctx.ReportDiagnostic(
                DiagnosticDescriptors.TargetEnumValueNotMapped,
                member.Name,
                member.ConstantValue!,
                ctx.Target,
                ctx.Source
            );
        }
    }

    public static void AddUnmappedSourceMembersDiagnostics(
        MappingBuilderContext ctx,
        ISet<IFieldSymbol> mappings,
        IEnumerable<IFieldSymbol> sourceMembers
    )
    {
        var missingSourceMembers = sourceMembers.Where(field => !mappings.Contains(field));
        foreach (var member in missingSourceMembers)
        {
            ctx.ReportDiagnostic(
                DiagnosticDescriptors.SourceEnumValueNotMapped,
                member.Name,
                member.ConstantValue!,
                ctx.Source,
                ctx.Target
            );
        }
    }
}
