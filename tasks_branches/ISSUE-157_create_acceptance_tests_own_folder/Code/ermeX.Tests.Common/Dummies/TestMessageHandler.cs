// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/

using System;
using System.Threading;
using ermeX.Bus.Interfaces;

namespace ermeX.Tests.Common.Dummies
{
    internal class TestMessageHandler : IHandleMessages<DummyDomainEntity>
    {
        private DummyDomainEntity _lastEntityReceived;

        public TestMessageHandler(AutoResetEvent autoResetEvent)
        {
            HandlerId = Guid.NewGuid();
            AutoResetEvent = autoResetEvent;
        }

        public TestMessageHandler() : this(null)
        {
            HandlerId = Guid.NewGuid();
        }

        public DummyDomainEntity LastEntityReceived
        {
            get { return _lastEntityReceived; }
        }

        public Guid HandlerId { get; set; }
        public AutoResetEvent AutoResetEvent { get; set; }


        #region IHandleMessages<DummyDomainEntity> Members

        public void HandleMessage(DummyDomainEntity message)
        {
            _lastEntityReceived = message;
            if (AutoResetEvent != null)
                AutoResetEvent.Set();
        }

        public bool Evaluate(DummyDomainEntity message)
        {
            return true;
        }

        #endregion

        public void Clear()
        {
            _lastEntityReceived = null;
        }
    }
}