using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ermeX.Threading.Queues
{
    /// <summary>
    /// Queues a task to be executed
    /// </summary>
    internal class SystemTaskQueue:ProducerParallelConsumerQueue<Action>
    {
        public SystemTaskQueue():base(1,64,3,TimeSpan.FromSeconds(60)){}

        protected override Action<Action> RunActionOnDequeue
        {
            get { return action => action(); }
        }
    }
}
