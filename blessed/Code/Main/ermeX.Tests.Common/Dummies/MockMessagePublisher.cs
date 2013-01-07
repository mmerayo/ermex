// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/

using System;
using ermeX.Bus.Interfaces;
using ermeX.Interfaces;
using ermeX.LayerMessages;

namespace ermeX.Tests.Common.Dummies
{
    internal class MockMessagePublisher : IMessagePublisher
    {
        private BusMessage _lastPublishedMessage;

        public object LastPublishedMessage
        {
            get { return _lastPublishedMessage; }
        }

        public void PublishMessage(BusMessage message)
        {
            _lastPublishedMessage = message;
        }

        public void Start()
        {
        }

        public TServiceInterface GetServiceProxy<TServiceInterface>() where TServiceInterface : IService
        {
            throw new NotImplementedException();
        }

        public TServiceInterface GetServiceProxy<TServiceInterface>(Guid componentId) where TServiceInterface : IService
        {
            throw new NotImplementedException();
        }
    }
}