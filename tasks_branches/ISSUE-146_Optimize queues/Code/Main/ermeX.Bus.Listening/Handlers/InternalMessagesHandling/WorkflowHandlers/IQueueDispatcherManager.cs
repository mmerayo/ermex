using System;

namespace ermeX.Bus.Listening.Handlers.InternalMessagesHandling.WorkflowHandlers
{
    internal interface IQueueDispatcherManager
    {
        void EnqueueItem(QueueDispatcherManager.QueueDispatcherManagerMessage message);
        event Action<Guid, object> DispatchMessage;
    }
}