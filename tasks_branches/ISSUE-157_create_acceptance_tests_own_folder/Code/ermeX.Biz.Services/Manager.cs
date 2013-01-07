// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using Common.Logging;
using Ninject;
using ermeX.Biz.Interfaces;
using ermeX.Bus.Interfaces;
using ermeX.ConfigurationManagement.Settings;
using ermeX.Interfaces;

namespace ermeX.Biz.Services
{
    internal class Manager : IServicesManager
    {
        [Inject]
        public Manager(IMessagePublisher publisher, IMessageListener listener)
        {
            if (publisher == null) throw new ArgumentNullException("publisher");
            if (listener == null) throw new ArgumentNullException("listener");
            Publisher = publisher;
            Listener = listener;
        }

        private IMessagePublisher Publisher { get; set; }
        private IMessageListener Listener { get; set; }
        private readonly ILog Logger = LogManager.GetLogger(StaticSettings.LoggerName);

        #region IServicesManager Members

        public TServiceInterface GetServiceProxy<TServiceInterface>() where TServiceInterface : IService
        {
            try
            {
                return Publisher.GetServiceProxy<TServiceInterface>();
            }catch(Exception ex)
            {
                Logger.Error(x=>x( "GetServiceProxy", ex));
                throw ex;
            }
        }

        /// <summary>
        ///   When there are several components publishing the same sevice, i.e. system services it specifies concretely which component to get the proxy for
        /// </summary>
        /// <typeparam name="TServiceInterface"> </typeparam>
        /// <param name="componentId"> </param>
        /// <returns> </returns>
        public TServiceInterface GetServiceProxy<TServiceInterface>(Guid componentId) where TServiceInterface : IService
        {
            try
            {
                return Publisher.GetServiceProxy<TServiceInterface>(componentId);
            }
            catch (Exception ex)
            {
                Logger.Error(x=>x( "GetServiceProxy", ex));
                throw ex;
            }
        }

        #endregion
    }
}