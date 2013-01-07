// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using Ninject;
using ermeX.Biz.Interfaces;
using ermeX.Bus.Interfaces;
using ermeX.Interfaces;

namespace ermeX.Biz.ServicesPublishing
{
    internal class Manager : IServicePublishingManager
    {
        [Inject]
        public Manager(IMessagePublisher publisher, IMessageListener listener, IDialogsManager dialogsManager)
        {
            if (publisher == null) throw new ArgumentNullException("publisher");
            if (listener == null) throw new ArgumentNullException("listener");
            if (dialogsManager == null) throw new ArgumentNullException("dialogsManager");
            Publisher = publisher;
            Listener = listener;
            DialogsManager = dialogsManager;
        }

        private IMessagePublisher Publisher { get; set; }
        private IMessageListener Listener { get; set; }
        private IDialogsManager DialogsManager { get; set; }

        #region IServicePublishingManager Members

        public void PublishService<TServiceInterface>(Type serviceImplementationType) where TServiceInterface : IService
        {
            Listener.PublishService<TServiceInterface>(serviceImplementationType);
            DialogsManager.NotifyService<TServiceInterface>(serviceImplementationType);
        }

        public void PublishService<TServiceInterface>() where TServiceInterface : IService
        {
            Listener.PublishService<TServiceInterface>();
        }

        public void PublishService(Type serviceInterface, Type serviceImplementation)
        {
            //TODO: THIS IS REDUNDANT
            Listener.PublishService(serviceInterface, serviceImplementation);
            DialogsManager.NotifyService(serviceInterface,serviceImplementation);

        }

        #endregion
    }
}