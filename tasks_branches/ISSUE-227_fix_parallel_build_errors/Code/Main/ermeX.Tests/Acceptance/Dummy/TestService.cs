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
using System.IO;
using System.Threading;
using ermeX.Common;
using ermeX.Tests.Acceptance.Dummy;

namespace ermeX.Tests.Acceptance.Dummy
{
    internal class TestService :MarshalByRefObject, ITestService1, ITestService2,ITestService3
    {
        private static readonly string fileName = Path.Combine(PathUtils.GetApplicationFolderPath(),
                                                               "TestTracker.delete");


        private static TrackerData tr = new TrackerData();

        public static TrackerData Tracker
        {
            get { return tr; }
        }

        #region ITestService Members

        public void EmptyMethod()
        {
            tr.ParametersLastCall.Clear();
            tr.ResultLastCall = null;
            tr.EmptyMethodCalled++;
            SerializeTracker();
            CheckExpectedRequests();
        }

        

        public AcceptanceMessageType1 ReturnMethod()
        {
            tr.ParametersLastCall.Clear();
            tr.ResultLastCall = null;
            tr.ReturnMethodCalled++;
            var result = new AcceptanceMessageType1();
            tr.ResultLastCall = result;
            SerializeTracker();
            CheckExpectedRequests();
           
            return result;
        }

        public AcceptanceMessageType1[] ReturnArrayMethod()
        {
            tr.ParametersLastCall.Clear();
            tr.ResultLastCall = null;
            tr.ReturnArrayMethodCalled++;
            var result = new[]
                             {
                                 new AcceptanceMessageType1(), new AcceptanceMessageType1(), new AcceptanceMessageType1(),
                                 new AcceptanceMessageType1()
                             };
            tr.ResultLastCall = result;
            SerializeTracker();
            CheckExpectedRequests();

            return result;
        }


        public void EmptyMethodWithOneParameter(AcceptanceMessageType1 param1)
        {
            tr.ParametersLastCall.Clear();
            tr.ResultLastCall = null;
            tr.EmptyMethodWithOneParameterCalled++;
            tr.ParametersLastCall.Add(param1);
            SerializeTracker();
            CheckExpectedRequests();
        }

        public AcceptanceMessageType2 ReturnMethodWithOneParameter(AcceptanceMessageType1 param1)
        {
            tr.ParametersLastCall.Clear();
            tr.ResultLastCall = null;
            tr.ReturnMethodWithOneParameterCalled++;
            tr.ParametersLastCall.Add(param1);
            var result = new AcceptanceMessageType2();
            tr.ResultLastCall = result;

            SerializeTracker();
            CheckExpectedRequests();
            return result;
        }

        public void EmptyMethodWithSeveralParameters(AcceptanceMessageType1 param1, AcceptanceMessageType2 param2)
        {
            tr.EmptyMethodWithSeveralParametersCalled++;
            tr.ParametersLastCall.Clear();
            tr.ResultLastCall = null;
            tr.EmptyMethodWithOneParameterCalled++;
            tr.ParametersLastCall.Add(param1);
            tr.ParametersLastCall.Add(param2);
            SerializeTracker();
            CheckExpectedRequests();
        }

        public AcceptanceMessageType3 ReturnMethodWithSeveralParameters(AcceptanceMessageType1 param1, AcceptanceMessageType2 param2)
        {
            tr.ReturnMethodWithSeveralParametersCalled++;
            tr.ParametersLastCall.Clear();
            tr.ResultLastCall = null;
            tr.ParametersLastCall.Add(param1);
            tr.ParametersLastCall.Add(param2);
            var result = new AcceptanceMessageType3();
            tr.ResultLastCall = result;

            SerializeTracker();
            CheckExpectedRequests();
            return result;
        }

        public AcceptanceMessageType3 ReturnMethodWithSeveralParametersParams(params AcceptanceMessageType1[] parameters)
        {
            tr.ReturnMethodWithArrayParametersCalled++;
            tr.ParametersLastCall.Clear();
            tr.ResultLastCall = null;
            foreach (var acceptanceMessageType1 in parameters)
                tr.ParametersLastCall.Add(acceptanceMessageType1);
           
            var result = new AcceptanceMessageType3();
            tr.ResultLastCall = result;

            SerializeTracker();
            CheckExpectedRequests();
            return result;
        }

       
        #endregion

        public static void Refresh()
        {
            tr = ObjectSerializer.DeserializeObjectWithoutOptimization<TrackerData>(fileName);
        }

        private static void SerializeTracker()
        {
            if (File.Exists(fileName))
                File.Delete(fileName);
            ObjectSerializer.SerializeObjectWithoutOptimization(fileName, Tracker);
        }

        public static void Reset()
        {
            tr=new TrackerData();
            SerializeTracker();
            _numRequestsToNotify = 0;
            _numRequestsReceived = 0;
            _receptionCompletedHandler = null;
        }

        private static int _numRequestsToNotify = 0;
        private static int _numRequestsReceived = 0;
        private static AutoResetEvent _receptionCompletedHandler;
        public static void NotifyWhenRequestsReceived(int numOfRequestsToReceive, AutoResetEvent requestsHandler)
        {
            _numRequestsToNotify = numOfRequestsToReceive;
            _receptionCompletedHandler = requestsHandler;

            if (numOfRequestsToReceive == 0)
                requestsHandler.Set();
        }

        private static void CheckExpectedRequests()
        {
            if (_receptionCompletedHandler == null)
                return;

            if (++_numRequestsReceived == _numRequestsToNotify)
                _receptionCompletedHandler.Set();
        }


        #region Nested type: TrackerData

        public class TrackerData:MarshalByRefObject
        {
            public List<AcceptanceMessageType> ParametersLastCall = new List<AcceptanceMessageType>();
            public object ResultLastCall = null;

            internal TrackerData()
            {
            }

            public int EmptyMethodCalled { get; set; }
            public int ReturnMethodCalled { get; set; }
            public int EmptyMethodWithOneParameterCalled { get; set; }
            public int ReturnMethodWithOneParameterCalled { get; set; }
            public int EmptyMethodWithSeveralParametersCalled { get; set; }
            public int ReturnMethodWithSeveralParametersCalled { get; set; }

            public int ReturnMethodWithArrayParametersCalled { get; set; }

            public int ReturnArrayMethodCalled { get; set; }

            public void Reset()
            {
                ParametersLastCall.Clear();
                ResultLastCall = null;
                EmptyMethodCalled =
                    ReturnMethodCalled =
                    EmptyMethodWithOneParameterCalled =
                    ReturnMethodWithOneParameterCalled =
                    EmptyMethodWithSeveralParametersCalled =
                    ReturnMethodWithSeveralParametersCalled =
                    ReturnMethodWithArrayParametersCalled = ReturnArrayMethodCalled = 0;
            }
        }

        #endregion
    }
}