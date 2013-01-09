namespace ermeX.Threading.Queues
{
    /// <summary>
    /// One only consumer per queue guarantees sequence
    /// </summary>
    /// <typeparam name="TQueueItem"></typeparam>
    internal abstract class ProducerSequentialConsumerQueue<TQueueItem>: ProducerParallelConsumerQueue<TQueueItem>
    {
        protected ProducerSequentialConsumerQueue():base(1,1)
        {}
    }
}