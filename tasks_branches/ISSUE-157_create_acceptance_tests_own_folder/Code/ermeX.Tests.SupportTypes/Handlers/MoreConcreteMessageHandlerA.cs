// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using ermeX.Bus.Interfaces;
using ermeX.Tests.SupportTypes.Messages;

namespace ermeX.Tests.SupportTypes.Handlers
{
    public class MoreConcreteMessageHandlerA : MessageHandlerA
    {
        //TODO SUPPORT TO SUBSCRIBE THIS HANDLER
        public override void HandleMessage(MessageA message)
        {
            throw new System.NotImplementedException();
        }

        //public bool Evaluate(MessageA message)
        //{
        //    throw new System.NotImplementedException();
        //}
    }
}
