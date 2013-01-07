// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;

using ermeX.Threading;

namespace ermeX.Tests.Common.Threading
{
    class DummyWorker:Worker
    {
        public DummyWorker(TimeSpan pollingTime)
            : base("DummyWorker", pollingTime)
        {
            
        }

        public DummyWorker()
            : base("DummyWorker")
        {
            
        }


        private volatile int _invokedTimes;
        public int InvokedTimes
        {
            get { return _invokedTimes; }
            private set { _invokedTimes = value; }
        }

        public void Reset()
        {
            InvokedTimes = 0;
        }

        protected override void DoWork(object data)
        {
            InvokedTimes++;
        }

        
    }
}
