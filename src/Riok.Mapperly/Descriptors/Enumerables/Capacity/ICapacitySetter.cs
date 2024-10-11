using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Symbols.Members;

namespace Riok.Mapperly.Descriptors.Enumerables.Capacity;

/// <summary>
/// Sets the capacity of a collection to the calculated count.
/// </summary>
public interface ICapacitySetter
{
    IMappableMember? CapacityTargetMember { get; }

    StatementSyntax Build(TypeMappingBuildContext ctx, ExpressionSyntax target);
}
