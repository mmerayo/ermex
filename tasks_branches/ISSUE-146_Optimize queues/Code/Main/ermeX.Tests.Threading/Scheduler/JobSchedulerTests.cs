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
using System.Text;
using System.Threading;
using NUnit.Framework;
using ermeX.Threading.Scheduling;

namespace ermeX.Tests.Threading.Scheduler
{
    [TestFixture]
    class JobSchedulerTests
    {
        readonly ManualResetEvent _jobCalledEvent=new ManualResetEvent(false);
        private bool _called = false;

        [SetUp]
        public void OnSetup()
        {
            _called = false;
            _jobCalledEvent.Reset();
        }

        [Test]
        public void Can_ScheduleJob([Values(1,2,3,4,5,20)]int seconds)
        {
           using(var target= new JobScheduler())
           {
               DateTime dateTime = DateTime.UtcNow;
               var sw=Stopwatch.StartNew();
               Job job = Job.At(dateTime.AddSeconds(seconds), ActionPayload);
               
               target.ScheduleJob(job);

               _jobCalledEvent.WaitOne(TimeSpan.FromSeconds(seconds + 1));
               TimeSpan elapsed = sw.Elapsed;
               Assert.IsTrue(_called);
               Console.WriteLine("Time elapsed: {0}", elapsed);
               Assert.LessOrEqual(elapsed.Seconds, seconds+1);
           }
        }

       

        [Test]
        public void Can_Dispose_When_ScheduledJobs_Pending()
        {
            const int seconds = 4;
            using (var target = new JobScheduler())
            {
                DateTime dateTime = DateTime.UtcNow;
                Job job = Job.At(dateTime.AddSeconds(seconds), ActionPayload);

                target.ScheduleJob(job);
            }
            _jobCalledEvent.WaitOne(TimeSpan.FromSeconds(seconds + 1));
            Assert.IsFalse(_called);
        }

        [Test]
        public void When_Dispose_Invoked_Waits_Running_ActionsToFinish()
        {
            using (var target = new JobScheduler())
            {
                DateTime dateTime = DateTime.UtcNow;
                Job job = Job.At(dateTime.AddMilliseconds(1), LongActionPayload);

                target.ScheduleJob(job);
                Thread.Sleep(250);
            }
            _jobCalledEvent.WaitOne(TimeSpan.FromSeconds(20));
            Assert.IsTrue(_called);
        }

        [Test]
        public void When_Dispose_Invoked_Non_Running_Actions_Are_Never_Started()
        {
            using (var target = new JobScheduler())
            {
                DateTime dateTime = DateTime.UtcNow;
                Job job = Job.At(dateTime.AddSeconds(120), ActionPayload);

                target.ScheduleJob(job);
                Thread.Sleep(250);
            }
            _jobCalledEvent.WaitOne(TimeSpan.FromSeconds(5));
            Assert.IsFalse(_called);
        }

        [Test]
        public void CanInvoke_Past_Actions()
        {
            using (var target = new JobScheduler())
            {
                DateTime dateTime = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1));
                Job job = Job.At(dateTime, ActionPayload);

                target.ScheduleJob(job);
                _jobCalledEvent.WaitOne(TimeSpan.FromSeconds(4));
                Assert.IsTrue(_called);
            }
        }

       

        private void ActionPayload()
        {
            Assert.IsFalse(_called);
            _called = true;
            _jobCalledEvent.Set();
        }

        private void LongActionPayload()
        {
            Thread.Sleep(TimeSpan.FromSeconds(5));
            ActionPayload();
        }

    }
}
