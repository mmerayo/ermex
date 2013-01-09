using System;
using System.Threading;
using NUnit.Framework;
using ermeX.Tests.Common.RandomValues;

namespace ermeX.Tests.Threading.Queues
{
    [TestFixture]
    internal abstract class TestProducerConsumerQueueBase
    {
        protected abstract ITestQueue GetTarget();

        [Test]
        public void CanEnqueueDequeue()
        {
            using (var target = GetTarget())
            {
                var expected = new DummyQueueItem(RandomHelper.GetRandomInt(), DateTime.UtcNow);
                target.EnqueueItem(expected);
                Thread.Sleep(2000);
                Assert.IsTrue(target.ItemsRead.Count == 1);
                Assert.AreSame(expected, target.ItemsRead[0]);
            }
        }

        
    }
}