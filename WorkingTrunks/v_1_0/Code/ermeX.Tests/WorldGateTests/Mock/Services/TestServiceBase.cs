// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using ermeX.Common;
using ermeX.Tests.Common.Dummies;

namespace ermeX.Tests.WorldGateTests.Mock
{
    internal class TestServiceBase : MarshalByRefObject
    {
        private static readonly string fileName = Path.Combine(PathUtils.GetApplicationFolderPath(),
                                                               "TestTracker.delete");

        private static TrackerData tr = new TrackerData();
        private static int _numRequestsToNotify = 0;
        private static int _numRequestsReceived = 0;
        private static AutoResetEvent _receptionCompletedHandler;

        public static TrackerData Tracker
        {
            get { return tr; }
        }

        public void EmptyMethod()
        {
            tr.ParametersLastCall.Clear();
            tr.EmptyMethodCalled++;
            SerializeTracker();
            CheckExpectedRequests();
        }

        public DummyDomainEntity ReturnMethod()
        {
            tr.ParametersLastCall.Clear();
            tr.ReturnMethodCalled++;
            SerializeTracker();
            CheckExpectedRequests();
            return new DummyDomainEntity {Id = Guid.NewGuid()};
        }

        public void EmptyMethodWithOneParameter(DummyDomainEntity param1)
        {
            tr.ParametersLastCall.Clear();
            tr.EmptyMethodWithOneParameterCalled++;
            tr.ParametersLastCall.Add(param1);
            SerializeTracker();
            CheckExpectedRequests();
        }

        public DummyDomainEntity ReturnMethodWithOneParameter(DummyDomainEntity param1)
        {
            tr.ParametersLastCall.Clear();
            tr.ReturnMethodWithOneParameterCalled++;
            tr.ParametersLastCall.Add(param1);
            SerializeTracker();
            CheckExpectedRequests();
            return new DummyDomainEntity {Id = Guid.NewGuid()};
        }

        public void EmptyMethodWithSeveralParameters(DummyDomainEntity param1, DummyDomainEntity param2)
        {
            tr.EmptyMethodWithSeveralParametersCalled++;
            tr.ParametersLastCall.Clear();
            tr.EmptyMethodWithOneParameterCalled++;
            tr.ParametersLastCall.Add(param1);
            tr.ParametersLastCall.Add(param2);
            SerializeTracker();
            CheckExpectedRequests();
        }

        public DummyDomainEntity ReturnMethodWithSeveralParameters(DummyDomainEntity param1, DummyDomainEntity param2)
        {
            tr.ReturnMethodWithSeveralParametersCalled++;
            tr.ParametersLastCall.Clear();
            tr.ParametersLastCall.Add(param1);
            tr.ParametersLastCall.Add(param2);
            SerializeTracker();
            CheckExpectedRequests();
            return new DummyDomainEntity {Id = Guid.NewGuid()};
        }

        public Guid ReturnMethodWithSeveralParametersValueTypes(Guid param1, DateTime param2)
        {
            tr.ReturnMethodWithSeveralParametersValueTypesCalled++;
            tr.ParametersLastCall.Clear();
            tr.ParametersLastCall.Add(param1);
            tr.ParametersLastCall.Add(param2);
            SerializeTracker();
            CheckExpectedRequests();
            return  Guid.NewGuid() ;
        }

        public MyCustomStruct ReturnCustomStructMethod(MyCustomStruct data)
        {
            tr.ReturnMethodWithCustomStructParametersCalled++;
            tr.ParametersLastCall.Clear();
            tr.ParametersLastCall.Add(data);
            SerializeTracker();
            CheckExpectedRequests();
            return data;
        }

        public EnumerationType ReturnEnumMethod(EnumerationType data)
        {
            tr.ReturnMethodWithEnumParametersCalled++;
            tr.ParametersLastCall.Clear();
            tr.ParametersLastCall.Add(data);
            SerializeTracker();
            CheckExpectedRequests();
            return data==EnumerationType.Value1? EnumerationType.Value2:EnumerationType.Value1;
        }


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

        public static void NotifyWhenRequestsReceived(int numOfRequestsToReceive, AutoResetEvent requestsHandler)
        {
            _numRequestsToNotify = numOfRequestsToReceive;
            _receptionCompletedHandler = requestsHandler;
        }

        private static void CheckExpectedRequests()
        {
            if (_receptionCompletedHandler == null)
                return;

            if (++_numRequestsReceived == _numRequestsToNotify)
                _receptionCompletedHandler.Set();
        }

        public class TrackerData:MarshalByRefObject
        {
            public List<object> ParametersLastCall = new List<object>();

            internal TrackerData()
            {
            }

            public int EmptyMethodCalled { get; set; }
            public int ReturnMethodCalled { get; set; }
            public int EmptyMethodWithOneParameterCalled { get; set; }
            public int ReturnMethodWithOneParameterCalled { get; set; }
            public int EmptyMethodWithSeveralParametersCalled { get; set; }
            public int ReturnMethodWithSeveralParametersCalled { get; set; }
            public int ReturnMethodWithSeveralParametersValueTypesCalled { get; set; }
            public int ReturnMethodWithCustomStructParametersCalled { get; set; }
            public int ReturnMethodWithEnumParametersCalled { get; set; }
        }
    }
}