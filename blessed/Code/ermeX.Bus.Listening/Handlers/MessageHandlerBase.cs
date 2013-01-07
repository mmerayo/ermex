// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using ermeX.Transport.Interfaces;

namespace ermeX.Bus.Listening.Handlers
{
    internal abstract class MessageHandlerBase<TMessage> : IServiceHandler
    {
        #region IServiceHandler Members

        public object Handle(object message)
        {
            return Handle((TMessage) message);
        }

        public abstract void Dispose();

        #endregion

        public abstract object Handle(TMessage message);
    }
}