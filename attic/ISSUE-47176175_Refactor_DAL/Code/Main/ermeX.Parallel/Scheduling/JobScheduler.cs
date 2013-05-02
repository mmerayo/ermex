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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Common.Logging;

namespace ermeX.Parallel.Scheduling
{
    internal sealed class JobScheduler : IJobScheduler
    {
        private readonly SortedList<long, List<Job>> _jobs;

        private readonly object _syncRoot;

        private int _queueRun;

        private int _jobsRunQueuedInThreadPool;

        private readonly Stopwatch _curTime;

        private readonly long _startTime;
        
        private readonly Timer _timer;
        
        private const long MinIncrement = 15*TimeSpan.TicksPerMillisecond;
        
        private const int MaxJobsToSchedulePerCheck = 128;

        private static readonly ILog Logger = LogManager.GetLogger(typeof(JobScheduler).FullName);

        public int Length
        {
            get
            {
                lock (_syncRoot)
                {
                    return _jobs.Count;
                }
            }
        }

        public JobScheduler()
        {
            _syncRoot = new object();

            _timer = new Timer(RunJobs);

            _startTime = DateTime.UtcNow.Ticks;
            _curTime = Stopwatch.StartNew();

            _jobs = new SortedList<long, List<Job>>();
        }
       
        public bool IsWorking()
        {
            return Interlocked.CompareExchange(ref _queueRun, 0, 0) == 1;
        }

        public IEnumerable<Job> GetJobs()
        {
            lock (_syncRoot)
            {
                return _jobs.Values.SelectMany(list => list).ToList().AsReadOnly();
            }
        }

        public void ScheduleJob(Job job)
        {
            lock (_syncRoot)
            {
                var shiftedTime = job.FireTime.Ticks - _startTime;

                List<Job> jobs;
                if (!_jobs.TryGetValue(shiftedTime, out jobs))
                {
                    jobs = new List<Job> {job};
                    _jobs.Add(shiftedTime, jobs);
                }
                else jobs.Add(job);


                if (Interlocked.CompareExchange(ref _queueRun, 1, 0) == 0)
                {
                    Interlocked.CompareExchange(ref _jobsRunQueuedInThreadPool, 1, 0);
                    ThreadPool.QueueUserWorkItem(RunJobs);
                }
                else
                {
                    long firetime = _jobs.Keys[0];
                    long delta = firetime - _curTime.Elapsed.Ticks;

                    if (delta < MinIncrement)
                    {
                        if (Interlocked.CompareExchange(ref _jobsRunQueuedInThreadPool, 1, 0) == 0)
                        {
                            _timer.Change(Timeout.Infinite, Timeout.Infinite);
                            ThreadPool.QueueUserWorkItem(RunJobs);
                        }
                    }
                    else
                    {
                        Logger.Debug(x=>x("Wake up time changed. Next event in {0}", TimeSpan.FromTicks(delta)));
                        _timer.Change(delta/TimeSpan.TicksPerMillisecond, Timeout.Infinite);
                    }
                }

            }
        }

        public void RemoveJobsByRequester(object requester)
        {
            if (requester == null) throw new ArgumentNullException("requester");
            lock (_syncRoot)
            {
                foreach (var job in _jobs)
                {
                    job.Value.RemoveAll(x => x.Requester == requester);
                }
            }
        }

        private void RunJobs(object state)
        {
            try
            {
                lock (_syncRoot)
                {
                    if (_disposed)
                        return;
                    Interlocked.CompareExchange(ref _jobsRunQueuedInThreadPool, 0, 1);

                    int availWorkerThreads;
                    int availCompletionPortThreads;

                    ThreadPool.GetAvailableThreads(out availWorkerThreads, out availCompletionPortThreads);

                    int jobsAdded = 0;

                    while (jobsAdded < MaxJobsToSchedulePerCheck && availWorkerThreads > MaxJobsToSchedulePerCheck + 1 &&
                           _jobs.Count > 0)
                    {
                        List<Job> curJobs = _jobs.Values[0];
                        long firetime = _jobs.Keys[0];

                        if (_curTime.Elapsed.Ticks <= firetime) break;

                        while (curJobs.Count > 0 && jobsAdded < MaxJobsToSchedulePerCheck &&
                               availWorkerThreads > MaxJobsToSchedulePerCheck + 1)
                        {
                            var job = curJobs[0];

                            if (job.DoAction != null)
                            {
                                ThreadPool.QueueUserWorkItem(callBack => job.DoAction(), job);
                                ++jobsAdded;

                                ThreadPool.GetAvailableThreads(out availWorkerThreads, out availCompletionPortThreads);
                            }

                            curJobs.Remove(job);
                        }

                        if (curJobs.Count < 1) _jobs.RemoveAt(0);
                    }

                    if (_jobs.Count > 0)
                    {
                        long firetime = _jobs.Keys[0];

                        long delta = firetime - _curTime.Elapsed.Ticks;

                        if (delta < MinIncrement)
                        {
                            if (Interlocked.CompareExchange(ref _jobsRunQueuedInThreadPool, 1, 0) == 0)
                            {
                                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                                ThreadPool.QueueUserWorkItem(RunJobs);
                            }
                        }
                        else
                        {
                            Logger.Debug(x => x("Next event in {0}", TimeSpan.FromTicks(delta)));
                            _timer.Change(delta/TimeSpan.TicksPerMillisecond, Timeout.Infinite);
                        }
                    }
                    else
                    {
                        Logger.Debug(x => x("Queue ends"));
                        Interlocked.CompareExchange(ref _queueRun, 0, 1);
                    }
                }
            }catch(Exception ex)
            {
                Logger.Warn(x=>x("{0}",ex.ToString()));
            }
        }

        #region IDisposable

        private bool _disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            lock (_syncRoot)
            {
                if (disposing)
                {
                    _curTime.Stop();
                    _timer.Dispose();

                }
                _disposed = true;
            }

        }
        ~JobScheduler()
        {
            Dispose(false);
        }

        #endregion

        
    }
}