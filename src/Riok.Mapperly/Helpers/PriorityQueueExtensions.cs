namespace Riok.Mapperly.Helpers;

internal static class PriorityQueueExtensions
{
    /// <summary>
    /// Dequeues all nodes.
    /// The nodes with the highest priority are returned first ordered in a FIFO fashion.
    /// Items added while this operation is in progress are also considered.
    /// </summary>
    /// <returns>An enumerable with all items.</returns>
    public static IEnumerable<TElement> DequeueAll<TElement, TPriority>(this PriorityQueue<TElement, TPriority> queue)
    {
        while (queue.TryDequeue(out var element, out _))
        {
            yield return element;
        }
    }
}
