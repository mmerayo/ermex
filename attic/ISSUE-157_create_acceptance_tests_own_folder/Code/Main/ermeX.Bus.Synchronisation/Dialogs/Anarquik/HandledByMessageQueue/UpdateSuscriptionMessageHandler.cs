// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using Ninject;
using ermeX.Bus.Interfaces;
using ermeX.Bus.Synchronisation.Dialogs.HandledByMessageQueue;
using ermeX.Bus.Synchronisation.Messages;

namespace ermeX.Bus.Synchronisation.Dialogs.Anarquik.HandledByMessageQueue
{
    internal class UpdateSuscriptionMessageHandler : IUpdateSuscriptionMessageHandler
    {
        [Inject]
        public UpdateSuscriptionMessageHandler(IMessagePublisher publisher, IMessageListener listener)
        {
            if (publisher == null) throw new ArgumentNullException("publisher");
            if (listener == null) throw new ArgumentNullException("listener");
            Publisher = publisher;
            Listener = listener;
            Type handlerInterfaceType = GetType().GetInterface(typeof(IHandleMessages<>).FullName);
            Listener.Suscribe(handlerInterfaceType, this);
        }

        private IMessagePublisher Publisher { get; set; }

        private IMessageListener Listener { get; set; }

        #region IUpdateSuscriptionMessageHandler Members

        public void HandleMessage(UpdateSuscriptionMessage message)
        {
            throw new NotImplementedException();
        }

        //public bool Evaluate(UpdateSuscriptionMessage message)
        //{
        //    throw new NotImplementedException();
        //}

        #endregion
    }
}