namespace Riok.Mapperly.Helpers;

internal static class QueueExtensions
{
    /// <summary>
    /// Dequeues all nodes.
    /// Items added while this operation is in progress are also considered.
    /// </summary>
    /// <returns>An enumerable with all items.</returns>
    public static IEnumerable<TElement> DequeueAll<TElement>(this Queue<TElement> queue)
    {
        while (queue.TryDequeue(out var element))
        {
            yield return element;
        }
    }
}
