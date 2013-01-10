namespace ermeX.Bus.Publishing.Dispatching.Messages
{
    internal interface IMessageDistributor
    {
        /// <summary>
        /// Number of threads active currently
        /// </summary>
        int CurrentThreadNumber { get; }

        int Count { get; }
        void EnqueueItem(MessageDistributor.MessageDistributorMessage item);
        void Dispose();
    }
}