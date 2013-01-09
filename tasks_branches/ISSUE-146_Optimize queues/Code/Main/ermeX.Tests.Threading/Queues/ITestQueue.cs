using System;
using System.Collections.Generic;

namespace ermeX.Tests.Threading.Queues
{
    internal interface ITestQueue:IDisposable
    {
        List<DummyQueueItem> ItemsRead { get; }
        int Count { get; }
        int CurrentThreadNumber { get; }
        void EnqueueItem(DummyQueueItem expected);

    }
}