// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/

using System;
using ermeX.Bus.Interfaces;
using ermeX.Bus.Interfaces.Dispatching;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Component;

namespace ermeX.Tests.Common.Dummies
{
    internal class DummyClientConfigurationSettings : ITransportSettings,IBusSettings
    {

        public Guid ComponentId { get; set; }

        public int CacheExpirationSeconds { get; set; }

        public NetworkingMode NetworkingMode
        {
            get { throw new NotImplementedException(); }
        }

        public FriendComponentData FriendComponent
        {
            get { throw new NotImplementedException(); }
        }

        public IEsbManager BusManager
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public IMessagePublisherDispatcherStrategy MessageDispatcher
        {
            get { throw new NotImplementedException(); }
        }

        

        public TimeSpan SendExpiringTime { get; set; }



        public int MaxDelayDueToLatencySeconds
        {
            get { throw new NotImplementedException(); }
        }

      

        public int MaxMessageKbBeforeChunking { get; set; }

        public ushort Port
        {
            get { throw new NotImplementedException(); }
        }

        public ushort MaxPortRange
        {
            get { throw new NotImplementedException(); }
        }

        public Type ConfigurationManagerType { get; set; }

        public bool DevLoggingActive
        {
            get { return true; }
        }

        public bool InternalDiagnosticsActive { get; set; }

       
    }
}