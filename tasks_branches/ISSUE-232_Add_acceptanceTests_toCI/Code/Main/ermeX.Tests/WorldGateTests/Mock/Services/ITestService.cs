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
using ermeX.Bus.Interfaces.Attributes;
using ermeX.Interfaces;
using ermeX.Tests.Common.Dummies;
using ermeX.Transport.Interfaces;

namespace ermeX.Tests.WorldGateTests.Mock
{
    [ServiceContract("8CC7D387-F944-486A-8405-59EF910EC869")]
    public interface ITestService : IService
    {
        [ServiceOperation("57EAC927-437A-4A09-91D8-0A034AD5E4F3")]
        void EmptyMethod();

        [ServiceOperation("45D6265F-1D2F-4EC5-853D-EC81DF66C31C")]
        DummyDomainEntity ReturnMethod();

        [ServiceOperation("D101C6E3-255C-4FE5-8578-E10D0AE683EE")]
        void EmptyMethodWithOneParameter(DummyDomainEntity param1);

        [ServiceOperation("502E2DC8-112C-451A-AF66-EFA3B6C1E0EC")]
        DummyDomainEntity ReturnMethodWithOneParameter(DummyDomainEntity param1);

        [ServiceOperation("BC2304FA-1721-42B8-AF0D-DDD5140E7425")]
        void EmptyMethodWithSeveralParameters(DummyDomainEntity param1, DummyDomainEntity param2);

        [ServiceOperation("592F3F00-DFB1-4C77-94F0-1719885EC7A3")]
        DummyDomainEntity ReturnMethodWithSeveralParameters(DummyDomainEntity param1, DummyDomainEntity param2);

       
        
    }

    //TODO: Replace WHEN supported generig services see issue-99
}