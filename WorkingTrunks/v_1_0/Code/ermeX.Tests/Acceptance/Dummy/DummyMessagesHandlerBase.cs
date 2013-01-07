// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Threading;

namespace ermeX.Tests.Acceptance.Dummy
{
    [Serializable]
    internal class DummyMessagesHandlerBase<TEntity> : MarshalByRefObject
    {
        public readonly List<TEntity> ReceivedMessages = new List<TEntity>();
        private AutoResetEvent _finishedEvent;
        private int _numMessages;

        public void HandleMessage(TEntity message)
        {
            ReceivedMessages.Add(message);
            Console.WriteLine("Received message of type: {0}", typeof(TEntity).FullName);
            if (ReceivedMessages.Count == _numMessages)
                _finishedEvent.Set();
        }

        public bool Evaluate(TEntity message)
        {
            return true;
        }

        public void NotifyWhenReceive(int numMessages, AutoResetEvent finishedEvent)
        {
            _numMessages = numMessages;
            _finishedEvent = finishedEvent;

            if (_numMessages == 0)
                finishedEvent.Set();
        }
    }
}