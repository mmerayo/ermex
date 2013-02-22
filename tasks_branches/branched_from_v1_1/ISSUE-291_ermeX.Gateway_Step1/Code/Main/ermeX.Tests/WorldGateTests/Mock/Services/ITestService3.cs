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
using ermeX;


namespace ermeX.Tests.WorldGateTests.Mock
{
    public enum EnumerationType
    {
        Value1,Value2
    }

    [ServiceContract("7DF58873-8822-4527-B67A-8014A0FC7CA5")]
    public interface ITestService3 : IService
    {
        [ServiceOperation("84E88870-4457-444D-839B-B3A2BBCB7C33")]
        Guid ReturnMethodWithSeveralParametersValueTypes(Guid param1, DateTime param2, int param3, uint param4,
                                                         short param5, ushort param6, long param7,ulong param8,float param9, double param10,decimal param11);

        [ServiceOperation("13CAFB26-D17C-4CE7-8667-AF7787E41172")]
        MyCustomStruct ReturnCustomStructMethod(MyCustomStruct data);

        [ServiceOperation("A647070F-A071-4E2A-B101-180EF63147C6")]
        EnumerationType ReturnEnumMethod(EnumerationType data);
        
    }
}