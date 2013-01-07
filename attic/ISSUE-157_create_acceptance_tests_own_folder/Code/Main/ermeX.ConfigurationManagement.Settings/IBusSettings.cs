// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ermeX.ConfigurationManagement.Settings.Component;

namespace ermeX.ConfigurationManagement.Settings
{
    internal interface IBusSettings:IComponentSettings   {
       
       int MaxDelayDueToLatencySeconds { get; }
       //IMessagePublisher Publisher { get; set; }
       /// <summary>
       ///   Time after what a message is expired and marked as failed
       /// </summary>
       TimeSpan SendExpiringTime { get; }
       NetworkingMode NetworkingMode { get; }

       FriendComponentData FriendComponent { get; }
    }
}
