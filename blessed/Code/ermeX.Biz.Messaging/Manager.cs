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


using ermeX.LayerMessages;

namespace ermeX.Biz.Messaging
{
    internal class Manager : IMessagingManager
    {
        [Inject]
        public Manager(IBusSettings settings, IMessagePublisher publisher, IMessageListener listener)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            if (publisher == null) throw new ArgumentNullException("publisher");
            if (listener == null) throw new ArgumentNullException("listener");
            Settings = settings;
            Publisher = publisher;
            Listener = listener;
        }

        private IBusSettings Settings { get; set; }
        private IMessagePublisher Publisher { get; set; }
        private IMessageListener Listener { get; set; }
        private readonly ILog Logger=LogManager.GetLogger(StaticSettings.LoggerName);

        #region IMessagingManager Members

        public void PublishMessage(BizMessage message)
        {
            try
            {
                var busMessage = new BusMessage(Settings.ComponentId, message);
                Logger.Trace(x=>x("{0} - Created BusMessage", busMessage.MessageId));
                Publisher.PublishMessage(busMessage);
            }
            catch (Exception ex)
            {
                Logger.Error(x=>x( "PublishMessage. {0}", ex));
                throw ex;
            }
        }

        #endregion

        
    }
}