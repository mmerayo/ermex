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
using System.Text;

namespace ermeX.Threading.Scheduling
{
    internal sealed class Job
    {
        private Job(){}

        public DateTime FireTime { get; private set; }

        public Action DoAction { get; private set; }
        public object Param { get; private set; }


        public static Job At(DateTime fireTime, Action doAction, object param = null)
        {
            return new Job {FireTime = fireTime.ToUniversalTime(), DoAction = doAction, Param = param};
        }

        public override string ToString()
        {
            return string.Format("{0}({1}) at {2}", DoAction != null ? DoAction.Method.Name : string.Empty, Param,
                                 FireTime.ToLocalTime().ToString("o"));
        }
    }
}
