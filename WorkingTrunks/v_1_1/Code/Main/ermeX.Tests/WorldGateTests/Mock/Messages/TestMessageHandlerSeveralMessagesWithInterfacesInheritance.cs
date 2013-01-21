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
using System.Threading;
using ermeX.Bus.Interfaces;
using ermeX.Tests.Common.Dummies;

namespace ermeX.Tests.WorldGateTests.Mock.Messages
{
    internal class TestMessageHandlerSeveralMessagesWithInterfacesInheritance: TestMessageHandlerBase, IHandleMessages<IDummyDomainEntity2>,IHandleMessages<DummyDomainEntity3>

    {

        public TestMessageHandlerSeveralMessagesWithInterfacesInheritance(int expectedMessages,AutoResetEvent autoResetEvent)
            : base(expectedMessages,autoResetEvent)
        {
        }

        public TestMessageHandlerSeveralMessagesWithInterfacesInheritance(int expectedMessages)
            : this(expectedMessages,null)
        {
        }

        public TestMessageHandlerSeveralMessagesWithInterfacesInheritance() : this(0) { }

        public void HandleMessage(DummyDomainEntity3 message)
        {
            UpdateHandledMessage(message);
        }

        public void HandleMessage(IDummyDomainEntity2 message)
        {
            UpdateHandledMessage(message);
        }
    }
}