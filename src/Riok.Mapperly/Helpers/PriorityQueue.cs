namespace Riok.Mapperly.Helpers;

/// <summary>
/// A simple implementation of a priority queue.
/// </summary>
/// <typeparam name="TElement">The type of the elements.</typeparam>
/// <typeparam name="TPriority">The priority of an element.</typeparam>
public class PriorityQueue<TElement, TPriority>
{
    private readonly Dictionary<TPriority, Queue<TElement>> _nodes = new();
    private readonly SortedSet<TPriority> _knownPriorities = new();

    public void Enqueue(TElement element, TPriority priority)
    {
        if (_nodes.TryGetValue(priority, out var node))
        {
            node.Enqueue(element);
            return;
        }

        var queue = new Queue<TElement>();
        queue.Enqueue(element);
        _nodes[priority] = queue;
        _knownPriorities.Add(priority);
    }

    /// <summary>
    /// Dequeues all nodes.
    /// The nodes with the highest priority are returned first ordered in a FIFO fashion.
    /// Items added while this operation is in progress are also considered.
    /// </summary>
    /// <returns>An enumerable with all items.</returns>
    public IEnumerable<TElement> DequeueAll()
    {
        while (_knownPriorities.Count > 0)
        {
            var priority = _knownPriorities.Max;
            var queue = _nodes[priority];
            var item = queue.Dequeue();

            if (queue.Count == 0)
            {
                _nodes.Remove(priority);
                _knownPriorities.Remove(priority);
            }

            yield return item;
        }
    }
}
