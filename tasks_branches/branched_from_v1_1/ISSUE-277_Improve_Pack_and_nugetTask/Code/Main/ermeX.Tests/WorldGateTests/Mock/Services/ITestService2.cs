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
using ermeX;

using ermeX.Tests.Common.Dummies;

namespace ermeX.Tests.WorldGateTests.Mock
{
    [ServiceContract("D21D9D92-3394-40B7-8E12-14625AF73C49")]
    public interface ITestService2 : IService
    {
        [ServiceOperation("AA86C421-1684-4A60-A39C-DC0535A320A8")]
        void EmptyMethod();

        [ServiceOperation("33015B8B-0BDE-45EE-9858-746EAC691BCF")]
        void EmptyMethodWithOneParameter(DummyDomainEntity param1);

        [ServiceOperation("789F7B3E-AD50-4A51-BACA-77994577C7F0")]
        void EmptyMethodWithSeveralParameters(DummyDomainEntity param1, DummyDomainEntity param2);
    }
}