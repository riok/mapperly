using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Helpers;

internal static class OperationExtensions
{
    public static IOperation? GetFirstChildOperation(this IOperation operation)
    {
#if ROSLYN4_4_OR_GREATER
        return operation.ChildOperations.FirstOrDefault();
#else
        return operation.Children.FirstOrDefault();
#endif
    }
}
