using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

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

    public static ISymbol? GetFieldOrProperty(this IOperation operation)
    {
        return operation switch
        {
            IFieldReferenceOperation fieldRefOperation => fieldRefOperation.Field,
            IPropertyReferenceOperation propertyRefOperation => propertyRefOperation.Property,
            _ => null,
        };
    }
}
