// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Linq;
using Ninject;
using ermeX.DAL.Interfaces;
using ermeX.Entities.Entities;

namespace ermeX.Bus.Listening.Handlers.InternalMessagesHandling.Schedulers
{
    internal class IncommingMessagesFifoScheduler : IScheduler
    {
        [Inject]
        public IncommingMessagesFifoScheduler(IIncomingMessagesDataSource dataSource, IBusMessageDataSource busMessageDataSource,
                                              IAppComponentDataSource componentsDataSource)
        {
            if (dataSource == null) throw new ArgumentNullException("dataSource");
            if (busMessageDataSource == null) throw new ArgumentNullException("busMessageDataSource");
            if (componentsDataSource == null) throw new ArgumentNullException("componentsDataSource");
            DataSource = dataSource;
            BusMessageDataSource = busMessageDataSource;
            ComponentsDataSource = componentsDataSource;
        }

        private IIncomingMessagesDataSource DataSource { get; set; }
        public IBusMessageDataSource BusMessageDataSource { get; set; }
        private IAppComponentDataSource ComponentsDataSource { get; set; }
        #region IScheduler Members

        public IncomingMessage GetNext()
        {
            var maxLatency = ComponentsDataSource.GetMaxLatency();
            return DataSource.GetNextDispatchableItem(maxLatency);
        }

        

        #endregion
    }
}