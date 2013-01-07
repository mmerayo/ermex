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
using ermeX.Bus.Listening.Handlers.InternalMessagesHandling;

namespace ermeX.Tests.Common.Dummies
{

    internal class DummyMessageHandler : MessageHandlerBase<DummyDomainEntity>
    {
        public static Guid OperationId = Guid.NewGuid();
        private readonly Dictionary<Guid, AutoResetEvent> _eventHandled = new Dictionary<Guid, AutoResetEvent>();

        private readonly Dictionary<Guid, DummyDomainEntity> _receivedMessages =
            new Dictionary<Guid, DummyDomainEntity>();

        public DummyMessageHandler(bool asInternalMessageHandler = true)
        {
            if (asInternalMessageHandler)
                OperationId = typeof(InternalMessageHandler).GUID;
        }


        public Dictionary<Guid, DummyDomainEntity> ReceivedMessages
        {
            get { return _receivedMessages; }
        }

        public override object Handle(DummyDomainEntity message)
        {
            _receivedMessages.Add(message.Id, message);
            if (_eventHandled.ContainsKey(message.Id))
                _eventHandled[message.Id].Set();
            return null;
        }

        public override void Dispose()
        {
        }

        public void AddEvent(Guid id, AutoResetEvent eventHandled)
        {
            _eventHandled.Add(id, eventHandled);
        }
    }
}