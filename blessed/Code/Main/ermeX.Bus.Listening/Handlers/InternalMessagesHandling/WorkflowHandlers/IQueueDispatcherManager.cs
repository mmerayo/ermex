using System;
using ermeX.Threading.Queues;

namespace ermeX.Bus.Listening.Handlers.InternalMessagesHandling.WorkflowHandlers
{
    internal interface IQueueDispatcherManager 
        //with this compiler was raising cyclic errors, why????: IProducerConsumerQueue<QueueDispatcherManager.QueueDispatcherManagerMessage>
    {
        void EnqueueItem(QueueDispatcherManager.QueueDispatcherManagerMessage item);
        event Action<Guid, object> DispatchMessage;
    }
}