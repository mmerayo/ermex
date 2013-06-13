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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ermeX;


namespace ermeX.Tests.SupportTypes.Services
{
    [ServiceContract("9E05015E-5E92-4F97-B70E-F0F89DE37AE8")]
    public interface IServiceA: IService
    {
        [ServiceOperation("96FC8562-3600-4AC6-88F5-0E2D39810767")]
        void EmptyMethod();
		[ServiceOperation("AB3AC0E7-4A38-4B98-98D5-1C34833A461B")]
	    long MethodReturnsTodayTicks();
    }
}
