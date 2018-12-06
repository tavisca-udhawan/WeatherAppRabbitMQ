using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Threading;
using Tavisca.Platform.Common.Internal;
using Xunit;

namespace Tavisca.Platform.Common.Tests
{
    public class RoundRobinPoolFixture
    {
        [Fact]
        public void Single_worker_process_should_use_same_thread_test()
        {
            int id1 = 0, id2 = 0;
            var pool = new RoundRobinPool(1);
            pool.Enqueue(() => id1 = Thread.CurrentThread.ManagedThreadId);
            pool.Enqueue(() => id2 = Thread.CurrentThread.ManagedThreadId);
            pool.StopAdding();
            id1.Should().Be(id2);
        }

        [Fact]
        public void Jobs_on_multiple_threads_should_distribute_in_round_robin_fashion_test()
        {
            var even = new List<int>();
            var odd = new List<int>();
            var pool = new RoundRobinPool(2);
            var waitHandle = new CountdownEvent(4);
            Action<List<int>> addItemTo = x =>
            {
                x.Add(Thread.CurrentThread.ManagedThreadId);
                waitHandle.Signal();
            };
            pool.Enqueue(() => addItemTo(odd));
            pool.Enqueue(() => addItemTo(even));
            pool.Enqueue(() => addItemTo(odd));
            pool.Enqueue(() => addItemTo(even));
            pool.StopAdding();

            waitHandle.Wait();
            even.Count.Should().Be(2);
            odd.Count.Should().Be(2);
            even[0].Should().Be(even[1]);
            odd[0].Should().Be(odd[1]);
            even[0].Should().NotBe(odd[0]);
        }





        private static Action Empty = () => { };

        [Fact]
        public void Adding_item_after_completion_should_throw()
        {
            var pool = new RoundRobinPool(1);
            pool.StopAdding();
            Action add = () => pool.Enqueue(Empty);
            add.ShouldThrow<InvalidOperationException>();
        }
    }
}
