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
using NUnit.Framework;

namespace ermeX.Tests.Threading.Worker
{
    [Explicit("TODO: Make these tests deterministic")]
    [TestFixture]
    public class WorkerTester
    {

        [Test]
        public void DoWork_IsNot_Invoked_Until_StartIs()
        {
            var target = new DummyWorker(TimeSpan.FromSeconds(1));
            
            Thread.Sleep(TimeSpan.FromSeconds(5));

            Assert.IsTrue(target.InvokedTimes == 0, string.Format("target.InvokedTimes: {0}", target.InvokedTimes));
        }

        [Test]
        public void DoWork_IsInvoked_UsingThePeriod()
        {
            var target = new DummyWorker(TimeSpan.FromSeconds(1));

            target.StartWorking(null);

            Thread.Sleep(TimeSpan.FromSeconds(5));

            Assert.IsTrue(target.InvokedTimes == 5, string.Format("target.InvokedTimes: {0}", target.InvokedTimes));

            target.Exit();

        }

        [Test]
        public void CanInvokeExitTwice()
        {
            var target = new DummyWorker(TimeSpan.FromSeconds(1));

            target.StartWorking(null);

            target.Exit();
            Assert.DoesNotThrow(target.Exit);

        }

        [Test]
        public void CantInvokeExit_and_StartAfter()
        {
            var target = new DummyWorker(TimeSpan.FromSeconds(1));

            target.StartWorking(null);
           
            target.Exit();
            
            Assert.Throws<ObjectDisposedException>(()=>target.StartWorking(null));


        }

        [Test]
        public void CantInvokeKill_And_StartAfter()
        {
            var target = new DummyWorker(TimeSpan.FromSeconds(1));

            target.StartWorking(null);


            target.Kill();
            Assert.Throws<ObjectDisposedException>(()=>target.StartWorking(null));
        }

        [Test]
        public void CanInvokeKillTwice()
        {
            var target = new DummyWorker(TimeSpan.FromSeconds(1));

            target.StartWorking(null);

            Thread.Sleep(TimeSpan.FromSeconds(5));

            Assert.IsTrue(target.InvokedTimes >= 5 && target.InvokedTimes <= 6, string.Format("target.InvokedTimes: {0}", target.InvokedTimes));

            target.Kill();
            Assert.DoesNotThrow(target.Kill);
           

        }


        [Test]
        public void DoWork_IsNot_Invoked_When_Period_IsNotSet()
        {
            var target = new DummyWorker();
            target.StartWorking(null);
            Thread.Sleep(TimeSpan.FromSeconds(5));

            Assert.IsTrue(target.InvokedTimes == 1, string.Format("target.InvokedTimes: {0}", target.InvokedTimes)); //1 as it does its first execution when started
        }


        [Test]
        public void When_Exit_Forces_To_Finish()
        {
            Assert.Inconclusive("TODO"); //TODO:
        }

        [Test]
        public void Only_Executes_DoWork_OneAtATime()
        {
            Assert.Inconclusive("TODO"); //TODO:
        }

        [Test]
        public void Notifies_DoWork_Finished()
        {
            var target = new DummyWorker(TimeSpan.FromSeconds(1));
            
            Stopwatch sw=new Stopwatch(); 
            sw.Start();

            target.StartWorking(null);

            
            target.FinishedWorkPendingEvent.WaitOne(TimeSpan.FromSeconds(5));

            sw.Stop();
            Assert.IsTrue(sw.ElapsedMilliseconds<5000);
            target.Exit();
        }

        [Test]
        public void Notifies_Thread_Finished()
        {
            var target = new DummyWorker(TimeSpan.FromSeconds(1));

            target.StartWorking(null);

            Thread.Sleep(500);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            target.Exit();

            target.FinishedEvent.WaitOne(TimeSpan.FromSeconds(30));
            sw.Stop();
            Assert.IsTrue(sw.ElapsedMilliseconds < 30000);
        }

    }
}
