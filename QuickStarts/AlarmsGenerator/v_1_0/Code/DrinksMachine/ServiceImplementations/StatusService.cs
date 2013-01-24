using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonContracts.Services;

namespace DrinksMachine.ServiceImplementations
{

    internal sealed class StatusService:IMachineStatusService
    {
        private static IStatusPublisher _publisher;

        /// <summary>
        /// Sets the status publisher
        /// </summary>
        /// <param name="publisher"></param>
        public static void SetStatusPublisher(IStatusPublisher publisher)
        {
            if (publisher == null) throw new ArgumentNullException("publisher");
            _publisher = publisher;
        }


        /// <summary>
        /// interface implementation,
        /// </summary>
        public void PublishStatus()
        {
            if(_publisher==null)
            {
                //log exception if there was a logging system
                return;
            }

            _publisher.PublishStatus();
        }
    }
}
