using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Tests.Helpers;

public class PriorityQueueExtensionsTest
{
    [Fact]
    public void DequeueAllShouldWork()
    {
        var queue = new PriorityQueue<char, int>();
        queue.Enqueue('C', 3);
        queue.Enqueue('A', 1);
        queue.Enqueue('B', 2);

        var index = 0;
        foreach (var item in queue.DequeueAll())
        {
            var expectedValue = (char)('A' + index);
            item.Should().Be(expectedValue);

            // enqueue during dequeue
            if (index == 2)
            {
                queue.Enqueue('F', 1);
                queue.Enqueue('D', 0);
                queue.Enqueue('E', 1);
            }

            index++;
        }

        index.Should().Be(6);
    }
}
