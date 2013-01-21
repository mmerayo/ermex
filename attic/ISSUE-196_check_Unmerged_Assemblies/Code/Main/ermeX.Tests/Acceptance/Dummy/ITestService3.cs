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
using System.Linq;
using System.Text;
using ermeX.Bus.Interfaces.Attributes;
using ermeX.Interfaces;

namespace ermeX.Tests.Acceptance.Dummy
{
    [ServiceContract("FD9A460C-F21F-4E50-92AE-AFE896C59A06")]
    public interface ITestService3 : IService
    {
        [ServiceOperation("E1C1C577-4FB5-48D7-B5FC-F9A5B84D31FF")]
        void EmptyMethod();

        [ServiceOperation("DD50D0A5-9E91-4E93-BCEF-75C4EB94C44F")]
        void EmptyMethodWithOneParameter(AcceptanceMessageType1 param1);

        [ServiceOperation("1E2CCCA1-D74B-4922-B82D-4E063066E119")]
        void EmptyMethodWithSeveralParameters(AcceptanceMessageType1 param1, AcceptanceMessageType2 param2);
    }
}
