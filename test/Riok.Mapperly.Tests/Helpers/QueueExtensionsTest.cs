using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Tests.Helpers;

public class QueueExtensionsTest
{
    [Fact]
    public void DequeueAllShouldDequeueAll()
    {
        var q = new Queue<int>();
        q.Enqueue(0);
        q.Enqueue(1);

        var index = 0;
        foreach (var item in q.DequeueAll())
        {
            item.Should().Be(index++);

            // enqueue during dequeue
            if (item == 0)
            {
                for (var i = 2; i < 10; i++)
                {
                    q.Enqueue(i);
                }
            }
        }

        index.Should().Be(10);
    }
}
