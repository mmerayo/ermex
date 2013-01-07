// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System.Threading;
using ermeX.Bus.Interfaces;
using ermeX.Tests.Common.Dummies;

namespace ermeX.Tests.WorldGateTests.Mock.Messages
{
    internal class TestMessageHandlerOneMessage : TestMessageHandlerBase, IHandleMessages<DummyDomainEntity>
    {

        public TestMessageHandlerOneMessage(int expectedMessages, AutoResetEvent autoResetEvent) : base(expectedMessages,autoResetEvent)
        {
        }

        public TestMessageHandlerOneMessage(int expectedMessages)
            : this(expectedMessages,null)
        {
        }

        public TestMessageHandlerOneMessage() : this(0) { }

        #region IHandleMessages<DummyDomainEntity> Members

        public void HandleMessage(DummyDomainEntity message)
        {
            UpdateHandledMessage(message);
        }

        #endregion
    }
}