// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/


using System;
using System.Collections.Generic;
using ermeX.Bus.Interfaces;
using ermeX.Bus.Interfaces.Dispatching;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Component;
using ermeX.Entities.Entities;

namespace ermeX.Tests.Common.Dummies
{
    internal class DummyEsbSettings : IBusSettings
    {
        public IEsbManager BusManager { get; set; }

        public IMessagePublisherDispatcherStrategy MessageDispatcher { get; set; }

       

        public int MaxDelayDueToLatencySeconds
        {
            get { throw new NotImplementedException(); }
        }

       
        public IEnumerable<AppComponent> SuscribersToLocalMessages
        {
            get { throw new NotImplementedException(); }
        }


        public Guid ComponentId { get; set; }

        public int CacheExpirationSeconds { get; set; }

        public Type ConfigurationManagerType
        {
            get { throw new NotImplementedException(); }
        }

        public bool DevLoggingActive
        {
            get { return true; }
        }

        public bool InternalDiagnosticsActive { get; set; }


        public TimeSpan SendExpiringTime
        {
            get { throw new NotImplementedException(); }
        }

        public NetworkingMode NetworkingMode
        {
            get { throw new NotImplementedException(); }
        }

        public FriendComponentData FriendComponent
        {
            get { throw new NotImplementedException(); }
        }
    }
}