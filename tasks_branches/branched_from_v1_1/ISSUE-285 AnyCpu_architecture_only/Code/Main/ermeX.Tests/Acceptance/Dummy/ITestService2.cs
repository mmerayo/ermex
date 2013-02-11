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

using ermeX.Transport.Interfaces;

namespace ermeX.Tests.Acceptance.Dummy
{
    [ServiceContract("6DBA0491-55DF-427A-BCEB-23A5BFB861FD")]
    public interface ITestService2 : IService
    {
        [ServiceOperation("4129CF2A-0328-459F-A412-EC4532180DE0")]
        void EmptyMethod();

        [ServiceOperation("4EF93E11-0E38-4023-BEA1-26EFF8C465F5")]
        AcceptanceMessageType1 ReturnMethod();

        [ServiceOperation("9933D2C9-3515-4EDE-8789-D08116BA956D")]
        void EmptyMethodWithOneParameter(AcceptanceMessageType1 param1);

        [ServiceOperation("60AB7606-7565-4B51-9AC9-8930CB34FF4F")]
        AcceptanceMessageType2 ReturnMethodWithOneParameter(AcceptanceMessageType1 param1);

        [ServiceOperation("3483576D-C4B2-410C-9BCD-AF5444EDFD21")]
        void EmptyMethodWithSeveralParameters(AcceptanceMessageType1 param1, AcceptanceMessageType2 param2);

        [ServiceOperation("5F738A1F-FD72-4049-A996-2DD4FA83ED81")]
        AcceptanceMessageType3 ReturnMethodWithSeveralParameters(AcceptanceMessageType1 param1, AcceptanceMessageType2 param2);

        [ServiceOperation("A2766F6B-14B6-4D2D-A2D3-F756E7C5F689")]
        AcceptanceMessageType3 ReturnMethodWithSeveralParametersParams(params AcceptanceMessageType1[] parameters);

        [ServiceOperation("1DD550BC-C899-475E-BD32-3FAA950158B0")]
        AcceptanceMessageType1[] ReturnArrayMethod();
    }
}