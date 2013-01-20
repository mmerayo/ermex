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
using ermeX.Transport.Interfaces;
using ermeX.Transport.Interfaces.Messages;

namespace ermeX.Bus.Listening.Handlers
{
    internal abstract class ServiceHandlerBase : IServiceHandler
    {
        #region IServiceHandler Members

        public object Handle(object message)
        {
            //TODO: DUE TO ServiceStack library issue
            var requestParameters = ((ServiceRequestMessage) message).Parameters;
            return Handle(requestParameters);
        }

        //private static void FixDueToIssueServiceStack(IDictionary<string, ServiceRequestMessage.RequestParameter> requestParameters)
        //{
        //    foreach (var key in requestParameters.Keys)
        //    {
        //        if (requestParameters[key].ParameterValue != null)
        //        {
        //            string toDeserialize = (string) requestParameters[key].ParameterValue;

        //            toDeserialize = toDeserialize.Substring(1, toDeserialize.Length - 2);//remove '[' and ']'

        //            if (toDeserialize.Contains("__type"))
        //            {
        //                var strings = toDeserialize.Split(',');
        //                var split = strings[0].Split('"');
        //                string typeName = split[3];
        //                strings[0] = split[0];

        //                var aux = new List<string>(strings);
        //                aux.RemoveAt(1);
        //                var finalString = string.Join(",", aux).Remove(1,1);

        //                requestParameters[key].ParameterValue =
        //                    ObjectSerializer.DeserializeObjectFromJson(finalString, typeName);
        //            }
        //            else
        //            {
        //                requestParameters[key].ParameterValue = toDeserialize;
        //            }
        //        }
        //    }
        //}

        public abstract void Dispose();

        #endregion

        public abstract object Handle(IDictionary<string, ServiceRequestMessage.RequestParameter> message);
    }
}