using System;
using ermeX.Threading.Queues;

namespace ermeX.Bus.Listening.Handlers.InternalMessagesHandling.WorkflowHandlers
{
    internal interface IQueueDispatcherManager : IProducerConsumerQueue<QueueDispatcherManager.QueueDispatcherManagerMessage>
    {
        event Action<Guid, object> DispatchMessage;
    }
}