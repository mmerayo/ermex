// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using ermeX.Bus.Listening.Handlers.InternalMessagesHandling.Workers;


using ermeX.Threading;

namespace ermeX.Tests.Services.Mock
{
    internal class MockMessagesDispatcherWorker :Worker, IIncomingMessagesDispatcherWorker
    {
        public MockMessagesDispatcherWorker()
            : base( "MockMessagesDispatcherWorker")
        {
        }

        public MockMessagesDispatcherWorker(string workerName, TimeSpan forceWorkPeriod) : base( workerName, forceWorkPeriod)
        {
        }

        public bool Started { get; private set; }
       
        public event Action<Guid, object> DispatchMessage;
        public override void StartWorking(object data)
        {
            Started = true;
            base.StartWorking(data);
        }
        protected override void DoWork(object data)
        {
            
        }
    }
}