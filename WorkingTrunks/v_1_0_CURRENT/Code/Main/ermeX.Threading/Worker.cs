// /*---------------------------------------------------------------------------------------*/
//        Licensed to the Apache Software Foundation (ASF) under one
//        or more contributor license agreements.  See the NOTICE file
//        distributed with this work for additional information
//        regarding copyright ownership.  The ASF licenses this file
//        to you under the Apache License, Version 2.0 (the
//        "License"); you may not use this file except in compliance
//        with the License.  You may obtain a copy of the License at
// 
//          http://www.apache.org/licenses/LICENSE-2.0
// 
//        Unless required by applicable law or agreed to in writing,
//        software distributed under the License is distributed on an
//        "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//        KIND, either express or implied.  See the License for the
//        specific language governing permissions and limitations
//        under the License.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Diagnostics;
using System.Threading;
using Common.Logging;
using ermeX.ConfigurationManagement.Settings;


namespace ermeX.Threading
{
    internal abstract class Worker : IWorker
    {
        //TODO: FORCE TO USE WORKER NAME
        public TimeSpan ForceWorkPeriod { get; private set; }
        private readonly AutoResetEvent _exitEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _workPendingEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _finishedEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _internalFinishedEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _finishedWorkPendingEvent = new AutoResetEvent(false);
        private readonly object _syncLock = new object();
        private volatile bool _working = false;
        private readonly AutoResetEvent _endPollingEvent = new AutoResetEvent(false);
        private Thread _poller = null;
        private Thread _workerThread = null;

        /// <summary>
        /// indicates the worker that must finish and dispose
        /// </summary>
        protected AutoResetEvent ExitEvent
        {
            get { return _exitEvent; }
        }

        /// <summary>r
        /// Indicates the worker that there are pending tasks to be done
        /// </summary>
        public AutoResetEvent WorkPendingEvent
        {
            get { return _workPendingEvent; }
        }

        public AutoResetEvent FinishedWorkPendingEvent
        {
            get { return _finishedWorkPendingEvent; }
        }

        /// <summary>
        /// Indicates the Client that the workers has finished and can be disposed if needed
        /// </summary>
        public AutoResetEvent FinishedEvent
        {
            get { return _finishedEvent; }
        }

        protected object SyncLock
        {
            get { return _syncLock; }
        }


        protected readonly ILog Logger = LogManager.GetLogger(StaticSettings.LoggerName);
        public string WorkerName { get; set; }


        private AutoResetEvent InternalFinishedEvent
        {
            get { return _internalFinishedEvent; }
        }

        protected Worker(string workerName)
            : this( workerName, TimeSpan.MinValue)
        {
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="workerName"> </param>
        /// <param name="forceWorkPeriod">period to invoke the method DoWork</param>
        protected Worker(string workerName, TimeSpan forceWorkPeriod)
        {
            if (string.IsNullOrEmpty(workerName.Trim()))
                throw new ArgumentException("workerName");

            WorkerName = workerName;
            ForceWorkPeriod = forceWorkPeriod;
            if (forceWorkPeriod == TimeSpan.MinValue)
                return;

        }

        public void Exit()
        {
            Dispose();
        }

        private void ExitThread(ref Thread thread, int seconds)
        {
            if (thread != null)
            {
                thread.Join(TimeSpan.FromSeconds(seconds));
                if (thread.IsAlive)
                    thread.Abort();
                thread = null;
            }
        }

        protected enum InvocationStatus
        {
            Begin = 1,
            End = 2,
            Exception
        }

//        [MethodImpl(MethodImplOptions.NoInlining)]
//        protected void LogDebug(InvocationStatus status)
//        {
//#if DEBUG

//            string text = string.Format("WorkerName:{1} Member:{0} Status:{3}", new StackFrame(1, true).GetMethod().Name, string.IsNullOrEmpty(WorkerName) ? "<<No named worker>>" : WorkerName, Environment.NewLine, status.ToString());
//            Logger.Debug(text);
//            Console.WriteLine(text);
//#endif
//        }

        public void Kill()
        {
            ExitEvent.Set();
            InternalFinishedEvent.WaitOne(TimeSpan.FromSeconds(3));
            if (_poller != null)
                _poller.Abort();
            if (_workerThread != null)
                _workerThread.Abort();
            FinishedEvent.Set();
            Dispose();

        }

        private void Start()
        {
            if (_poller != null)
            {
                _poller.Abort();
                _poller = null;
            }
            if (ForceWorkPeriod != TimeSpan.MinValue)
            {
                _poller = new Thread(DoPoll) {Name = WorkerName};

                _poller.Start();
            }
        }

        private void DoPoll()
        {
            try
            {
                do
                {
                    if (!_disposed && !_working && _workerThread != null && _workerThread.IsAlive)
                        WorkPendingEvent.Set();

                    EndPollingEvent.WaitOne(ForceWorkPeriod);
                } while (!_disposed && !_disposing);
            }
            catch (ThreadAbortException ex)
            {
                Logger.Warn(x=>x("DoPoll {0} Exception:{1} ", WorkerName, ex));

            }
            catch (Exception ex)
            {
                Logger.Warn(x => x("DoPoll {0} Exception:{1} ", WorkerName, ex));
            }

        }

        public virtual void StartWorking(object data)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (_workerThread != null)
                throw new InvalidOperationException("Must Exit the thread before");
            _workerThread = new Thread(TheRealWorker) {Name = WorkerName};
            _workerThread.Start(data);
            WorkPendingEvent.Set();

        }

        public bool ErrorResult { get; set; }

        private void TheRealWorker(object data)
        {

            var autoResetEvents = new WaitHandle[] {ExitEvent, WorkPendingEvent, FinishedEvent};
            const int exitEventIndex = 0;
            const int finishedEventIndex = 2;

            Start();
            try
            {
                int currEventIdx = -1;
                while (((currEventIdx = WaitHandle.WaitAny(autoResetEvents)) != exitEventIndex) &&
                       currEventIdx != finishedEventIndex) //while not exit requested from the client wait for message
                {
                    if (!_working)
                        lock (SyncLock)
                            if (!_working)
                            {
                                _working = true;
                                try
                                {
                                    DoWork(data);
                                }
                                catch (ThreadAbortException ex)
                                {
                                    Logger.Warn(x => x("DoWork {0} Exception:{1} ", WorkerName, ex));
                                }
                                catch (Exception ex)
                                {
                                    Logger.Warn(x => x("DoWork {0} Exception:{1} ", WorkerName, ex));
                                    Debugger.Break();
                                    throw ex;

                                }
                                finally
                                {
                                    _working = false;
                                    FinishedWorkPendingEvent.Set();
                                    OnPendingWorkFinished();
                                }

                            }
                }
            }
            catch (ThreadAbortException ex)
            {
                Logger.Warn(x => x("TheRealWorker {0} Exception:{1} ", WorkerName, ex));
                
            }
            catch (Exception ex)
            {
                Logger.Warn(x => x("TheRealWorker {0} Exception:{1} ", WorkerName, ex));
                ErrorResult = true;

            }
            finally
            {
                InternalFinishedEvent.Set();
                FinishedEvent.Set();
            }

        }


        protected abstract void DoWork(object data);



        private AutoResetEvent EndPollingEvent
        {
            get { return _endPollingEvent; }
        }

        private bool _disposed = false;
        private bool _disposing = false;

        public void Dispose()
        {
            if (!_disposed)
                lock (SyncLock)
                    if (!_disposed)
                    {
                        _disposing = true;
                        Dispose(_disposing);
                        GC.SuppressFinalize(this);
                    }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                EndPollingEvent.Set();
                ExitEvent.Set();
                InternalFinishedEvent.WaitOne(TimeSpan.FromSeconds(10));
                ExitThread(ref _poller, 1);
                ExitThread(ref _workerThread, 10);
                //FinishedEvent.WaitOne(TimeSpan.FromSeconds(10));
                FinishedEvent.Set();

                _disposed = true;
                OnThreadFinished();
                Logger.Info(x => x("Thread {0} disposed", WorkerName));
            }
        }

        ~Worker()
        {
            if (!_disposed)
            {
                Logger.Info(x => x("Thread {0} disposed from destructor", WorkerName));
                Dispose();
            }
        }

        #region events

        public EventHandler ThreadFinished { get; set; }

        private void OnThreadFinished()
        {
            if (ThreadFinished != null)
            {
                try
                {
                    ThreadFinished(this, null);
                }
                catch (Exception ex)
                {
                    Logger.Warn(x=>x("OnThreadFinished: {0} Exception: {1}", WorkerName, ex));
                    throw ex;
                }
            }
        }

        public EventHandler PendingWorkFinished { get; set; }

        private void OnPendingWorkFinished()
        {
            if (PendingWorkFinished != null)
            {

                new Thread(DoPendingWorkNotification).Start();

            }
        }

        private void DoPendingWorkNotification()
        {
            try
            {
                PendingWorkFinished(this, null);
            }
            catch (Exception ex)
            {
                Logger.Warn(x => x("OnPendingWorkFinished: {0} Exception: {1}", WorkerName, ex));
                throw ex;
            }
        }

        #endregion

    }
}
