using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Riok.Mapperly.Helpers;

internal static class OperationExtensions
{
    public static TOperation? GetFirstChildOperation<TOperation>(this IOperation operation)
        where TOperation : class, IOperation
    {
        return operation.ChildOperations.FirstOrDefault() as TOperation;
    }

    public static ISymbol? GetMemberSymbol(this IOperation operation)
    {
        return operation is IMemberReferenceOperation memberRefOperation ? memberRefOperation.Member : null;
    }
}
