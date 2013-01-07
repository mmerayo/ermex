// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ermeX.Tests.WorldGateTests.Mock.Messages
{
    internal abstract class TestMessageHandlerBase
    {
        private int _expectedMessages;
        private readonly List<object> _receivedMessages;

        protected TestMessageHandlerBase(int  expectedMessages, AutoResetEvent autoResetEvent)
        {
            ExpectedMessages = expectedMessages;
            HandlerId = Guid.NewGuid();
            AutoResetEvent = autoResetEvent;
            _receivedMessages = new List<object>();
        }

        protected TestMessageHandlerBase(int expectedMessages)
            : this(expectedMessages,null)
        {
        }

        protected TestMessageHandlerBase(): this(0, null){}

        public TMessage LastEntityReceived<TMessage>()
        {
            return (TMessage)ReceivedMessages.Last(x=>x.GetType()==typeof(TMessage)); 
        }

        public Guid HandlerId { get; set; }
        public static AutoResetEvent AutoResetEvent { get; set; }
        public void SetReceivedEvent(AutoResetEvent receivedEvent)
        {
            if (receivedEvent == null) throw new ArgumentNullException("receivedEvent");
            AutoResetEvent = receivedEvent;
        }

        public List<object> ReceivedMessages
        {
            get { return _receivedMessages; }
        }

        public int ExpectedMessages
        {
            get { return _expectedMessages; }
            set { _expectedMessages = value; }
        }

        public void Clear()
        {
            ExpectedMessages = 0;
            ReceivedMessages.Clear();
        }

        protected void UpdateHandledMessage(object message)
        {
            ReceivedMessages.Add(message);
            if (ExpectedMessages==ReceivedMessages.Count && AutoResetEvent != null)
                AutoResetEvent.Set();
        }
    }
}
