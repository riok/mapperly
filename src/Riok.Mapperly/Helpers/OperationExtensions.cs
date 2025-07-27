using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Riok.Mapperly.Helpers;

internal static class OperationExtensions
{
    public static TOperation? GetFirstChildOperation<TOperation>(this IOperation operation)
        where TOperation : class, IOperation
    {
#if ROSLYN4_4_OR_GREATER
        return operation.ChildOperations.FirstOrDefault() as TOperation;
#else
        return operation.Children.FirstOrDefault() as TOperation;
#endif
    }

    public static ISymbol? GetMemberSymbol(this IOperation operation)
    {
        return operation is IMemberReferenceOperation memberRefOperation ? memberRefOperation.Member : null;
    }
}
