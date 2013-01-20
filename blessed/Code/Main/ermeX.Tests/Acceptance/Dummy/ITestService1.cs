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

namespace ermeX.Tests.Acceptance.Dummy
{
    [ServiceContract("D6AA6A10-E8BC-4854-AEE8-7AEEDF2C6B1D")]
    public interface ITestService1 : IService
    {
        [ServiceOperation("D314C873-2E7A-4C4C-8A2E-06ED6BA49FDB")]
        void EmptyMethod();

        [ServiceOperation("C75BADE3-9F6C-42DD-817D-2DD1D086E8A2")]
        AcceptanceMessageType1 ReturnMethod();

        [ServiceOperation("24699691-965F-4ED3-A4ED-3B246F4CDB84")]
        void EmptyMethodWithOneParameter(AcceptanceMessageType1 param1);

        [ServiceOperation("17A2BA2E-DB32-4823-BEFE-7DCBDD87CC20")]
        AcceptanceMessageType2 ReturnMethodWithOneParameter(AcceptanceMessageType1 param1);

        [ServiceOperation("DE0011C6-A325-4A26-B4B1-0ED27F6038F6")]
        void EmptyMethodWithSeveralParameters(AcceptanceMessageType1 param1, AcceptanceMessageType2 param2);

        [ServiceOperation("9350C674-A530-4EE4-8C03-1DADD5D09F46")]
        AcceptanceMessageType3 ReturnMethodWithSeveralParameters(AcceptanceMessageType1 param1, AcceptanceMessageType2 param2);

        [ServiceOperation("C9C1C512-1011-4663-BAF2-482EB1D5E845")]
        AcceptanceMessageType3 ReturnMethodWithSeveralParametersParams(params AcceptanceMessageType1 [] parameters);

        [ServiceOperation("241C0073-C541-4395-B731-800842276054")]
        AcceptanceMessageType1[] ReturnArrayMethod();
    }
}