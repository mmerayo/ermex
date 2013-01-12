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
using System.Threading;
using NUnit.Framework;
using ermeX.Tests.Common.RandomValues;

namespace ermeX.Tests.Threading.Queues
{
    [TestFixture]
    internal abstract class TestProducerConsumerQueueBase
    {
        protected abstract ITestQueue GetTarget();

        [Test]
        public void CanEnqueueDequeue()
        {
            using (var target = GetTarget())
            {
                var expected = new DummyQueueItem(RandomHelper.GetRandomInt(), DateTime.UtcNow);
                target.EnqueueItem(expected);
                Thread.Sleep(2000);
                Assert.IsTrue(target.ItemsRead.Count == 1);
                Assert.AreSame(expected, target.ItemsRead[0]);
            }
        }

        
    }
}