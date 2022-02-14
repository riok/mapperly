namespace Riok.Mapperly.Helpers;

public static class QueueExtensions
{
    public static IEnumerable<T> DequeueAll<T>(this Queue<T> q)
    {
        while (q.Count > 0)
        {
            yield return q.Dequeue();
        }
    }
}
