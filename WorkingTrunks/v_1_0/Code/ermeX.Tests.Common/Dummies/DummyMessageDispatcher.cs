// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/

using ermeX.Bus.Interfaces;
using ermeX.Bus.Interfaces.Dispatching;
using ermeX.LayerMessages;

namespace ermeX.Tests.Common.Dummies
{
    internal class DummyMessageDispatcher : IMessagePublisherDispatcherStrategy
    {
        public BusMessage LastPublishedMessage { get; private set; }

        #region IMessagePublisherDispatcherStrategy Members

        public void Dispose()
        {
        }

        public DispatcherStatus Status { get; set; }

        

        public void Start()
        {
            Status = DispatcherStatus.Started;
        }

        public void Stop()
        {
            Status = DispatcherStatus.Stopped;
        }

        #endregion


        public void Dispatch(BusMessage messageToPublish)
        {
            LastPublishedMessage = messageToPublish;
        }

        public void Clear()
        {
            Status = DispatcherStatus.Stopped;
            LastPublishedMessage = null;
        }
    }
}