using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Logging;
using ermeX.Bus.Interfaces;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces;

namespace ermeX.Bus.Publishing.Dispatching.Messages
{
    /// <summary>
    /// 
    /// </summary>
    internal interface ISubscribersDispatcher
    {
        /// <summary>
        /// Number of threads active currently
        /// </summary>
        int CurrentThreadNumber { get; }

        int Count { get; }
        void EnqueueItem(SubscribersDispatcher.SubscribersDispatcherMessage item);
        void Dispose();
    }
}
