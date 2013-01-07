// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Threading;
using ermeX.Bus.Listening.Handlers;
using ermeX.Transport.Interfaces.Messages;

namespace ermeX.Tests.Common.Dummies
{
    internal class DummyServiceHandler : ServiceHandlerBase
    {
        public static Guid OperationId = Guid.NewGuid();
        private readonly AutoResetEvent _eventHandled;

        public DummyServiceHandler(AutoResetEvent eventHandled)
        {
            _eventHandled = eventHandled;
        }

        public IDictionary<string, ServiceRequestMessage.RequestParameter> ReceivedMessage { get; private set; }


        public override object Handle(IDictionary<string, ServiceRequestMessage.RequestParameter> message)
        {
            ReceivedMessage = message;
            _eventHandled.Set();
            return null;
        }

        public override void Dispose()
        {
        }
    }
}