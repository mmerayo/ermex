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
    internal class TestMessageHandlerSeveralMessages : TestMessageHandlerBase, IHandleMessages<DummyDomainEntity2>, IHandleMessages<DummyDomainEntity>
    {

        public TestMessageHandlerSeveralMessages(int expectedMessages,AutoResetEvent autoResetEvent)
            : base(expectedMessages,autoResetEvent)
        {
        }

        public TestMessageHandlerSeveralMessages(int expectedMessages)
            : this(expectedMessages,null)
        {
        }

        public TestMessageHandlerSeveralMessages() : this(0) { }
        public void HandleMessage(DummyDomainEntity message)
        {
            UpdateHandledMessage(message);
        }

        public void HandleMessage(DummyDomainEntity2 message)
        {
            UpdateHandledMessage(message);
        }
    }
}