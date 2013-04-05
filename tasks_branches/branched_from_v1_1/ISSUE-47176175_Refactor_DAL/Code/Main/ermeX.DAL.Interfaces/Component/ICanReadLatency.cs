namespace ermeX.Domain.Component
{
    internal interface ICanReadLatency
    {
        
        /// <summary>
        /// Gets the maximum latency in milliseconds
        /// </summary>
        /// <returns></returns>
        int GetMaxLatency();
    }
}
