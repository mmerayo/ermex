// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Threading;

namespace ermeX.Threading
{
    internal interface IWorker:IDisposable
    {
        /// <summary>
        /// Indicates the worker that there are pending tasks to be done
        /// </summary>
        AutoResetEvent WorkPendingEvent { get; }

        /// <summary>
        /// Indicates the client that there pending tasks have been done
        /// </summary>
        AutoResetEvent FinishedWorkPendingEvent { get; }

        /// <summary>
        /// Indicates the Client that the workers has finished and can be disposed if needed
        /// </summary>
        AutoResetEvent FinishedEvent { get; }

        /// <summary>
        /// Forces the worker to finish
        /// </summary>
        void Exit();

        /// <summary>
        /// Thats it but, please dont use
        /// </summary>
        void Kill();
        /// <summary>
        /// Starts the work
        /// </summary>
        /// <param name="data"></param>
        void StartWorking(object data);

        /// <summary>
        /// indicates whether it finished with error or not
        /// </summary>
        bool ErrorResult { get; }

        string WorkerName { get; set; }
        EventHandler ThreadFinished { get; set; }
        EventHandler PendingWorkFinished { get; set; }
    }
}