namespace Riok.Mapperly.Tests.Helpers;

public class PriorityQueueTest
{
    [Fact]
    public void EnqueueAndDequeueAllShouldWork()
    {
        var queue = new Mapperly.Helpers.PriorityQueue<char, int>();
        queue.Enqueue('C', 1);
        queue.Enqueue('A', 3);
        queue.Enqueue('B', 2);

        var index = 0;
        foreach (var item in queue.DequeueAll())
        {
            var expectedValue = (char)('A' + index);
            item.Should().Be(expectedValue);

            // enqueue during dequeue
            if (index == 2)
            {
                queue.Enqueue('E', 0);
                queue.Enqueue('D', 1);
                queue.Enqueue('F', 0);
            }

            index++;
        }

        index.Should().Be(6);
    }
}
