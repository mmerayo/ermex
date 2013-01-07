// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System.Threading;
using ermeX.Bus.Listening.Handlers.InternalMessagesHandling.Workers;
using ermeX.Common;

using ermeX.Threading;

namespace ermeX.Tests.Services.Mock
{
    internal class MockMessagesProcessorWorker : Worker, IIncomingMessagesProcessorWorker
    {
        public MockMessagesProcessorWorker()
            : base( "MockMessagesDispatcherWorker")
        {
        }

        public bool Started { get; private set; }

        public bool NewItemArrivedFlagged { get; private set; }

        public void Work(object events)
        {
            var syncEvents = ((SyncEvents[]) events)[0];

           

            int index;
            while ((index = WaitHandle.WaitAny((syncEvents).EventArray)) != SyncEvents.EXIT_INDEX)
            {
                if (index != SyncEvents.NewItemArrived)
                    continue;

               
            }
        }

        public override void StartWorking(object data)
        {
            Started = true;
           base.StartWorking(data);
        }

        protected override void DoWork(object data)
        {
            NewItemArrivedFlagged = true;
        }
    }
}