using System;

namespace ermeX.Bus.Listening.Handlers.InternalMessagesHandling.WorkflowHandlers
{
    internal interface IReceptionMessageDistributor:IDisposable
    {
        /// <summary>
        /// Number of threads active currently
        /// </summary>
        int CurrentThreadNumber { get; }

        int Count { get; }
        void EnqueueItem(ReceptionMessageDistributor.MessageDistributorMessage item);
    }
}