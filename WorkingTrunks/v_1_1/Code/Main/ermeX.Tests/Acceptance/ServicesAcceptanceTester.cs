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
using System.Threading;
using NUnit.Framework;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.Exceptions;
using ermeX.Tests.Acceptance.Dummy;
using ermeX.Tests.Common.Networking;
using ermeX.Tests.Common.SettingsProviders;


namespace ermeX.Tests.Acceptance
{
    [TestFixture]
    internal sealed class ServicesAcceptanceTester:AcceptanceTester
    {
        //TODO: implement async calls

        #region Local

        [Test,TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void CanInvoke_LocalCustomService_EmptyMethod(DbEngineType engineType)
        {
            string dbConnString = TestSettingsProvider.GetConnString(engineType);

            var c1Port = new TestPort(10001);
            
            using (var component1 = TestComponent.GetComponent(true))
            {
                InitializeLonelyComponent(engineType, c1Port, component1, dbConnString);

                var finishedEvent = new AutoResetEvent(false);
                var registeredEvent = new AutoResetEvent(false);
                component1.RegisterService(1, registeredEvent, finishedEvent);
                registeredEvent.WaitOne(TimeSpan.FromSeconds(10));

                var target = component1.GetServiceProxy<ITestService1>();
                target.EmptyMethod();
                finishedEvent.WaitOne(TimeSpan.FromSeconds(5));

                Assert.IsTrue(component1.Tracker.EmptyMethodCalled == 1);
                Assert.IsTrue(component1.Tracker.ParametersLastCall.Count == 0);
            }
        }

        [Test,TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void CanInvoke_LocalCustomService_OneParamMethod(DbEngineType engineType)
        {
            string dbConnString = TestSettingsProvider.GetConnString(engineType);

            var c1Port = new TestPort(10000);
            
            using (var component1 = TestComponent.GetComponent(true))
            {
                InitializeLonelyComponent(engineType, c1Port, component1, dbConnString);

                var finishedEvent = new AutoResetEvent(false);
                var registeredEvent = new AutoResetEvent(false);
                component1.RegisterService(1, registeredEvent, finishedEvent);
                registeredEvent.WaitOne(TimeSpan.FromSeconds(10));

                var target = component1.GetServiceProxy<ITestService1>();
                var expected = new AcceptanceMessageType1();

                target.EmptyMethodWithOneParameter(expected);
                finishedEvent.WaitOne(TimeSpan.FromSeconds(5));

                Assert.IsTrue(component1.Tracker.EmptyMethodWithOneParameterCalled == 1);
                Assert.IsTrue(component1.Tracker.ParametersLastCall.Count == 1);
                Assert.AreEqual(expected, component1.Tracker.ParametersLastCall[0]);
            }
        }

        [Test,TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void CanInvoke_LocalCustomService_SeveralParamsMethod(DbEngineType engineType)
        {
            string dbConnString = TestSettingsProvider.GetConnString(engineType);

            var c1Port = new TestPort(10000);
            
            using (var component1 = TestComponent.GetComponent(true))
            {
                InitializeLonelyComponent(engineType, c1Port, component1, dbConnString);

                var finishedEvent = new AutoResetEvent(false);
                var registeredEvent = new AutoResetEvent(false);
                component1.RegisterService(1, registeredEvent, finishedEvent);
                registeredEvent.WaitOne(TimeSpan.FromSeconds(10));

                var target = component1.GetServiceProxy<ITestService1>();
                var expected1 = new AcceptanceMessageType1();
                var expected2 = new AcceptanceMessageType2();

                target.EmptyMethodWithSeveralParameters(expected1, expected2);
                finishedEvent.WaitOne(TimeSpan.FromSeconds(5));

                Assert.IsTrue(component1.Tracker.EmptyMethodWithSeveralParametersCalled == 1);
                Assert.IsTrue(component1.Tracker.ParametersLastCall.Count == 2);
                Assert.AreEqual(expected1, component1.Tracker.ParametersLastCall[0]);
                Assert.AreEqual(expected2, component1.Tracker.ParametersLastCall[1]);
            }
        }


        [Test,TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void CanInvoke_LocalCustomService_EmptyMethod_ReturnValue(DbEngineType engineType)
        {
            string dbConnString = TestSettingsProvider.GetConnString(engineType);

            var c1Port = new TestPort(10000);
            
            using (var component1 = TestComponent.GetComponent(true))
            {
                InitializeLonelyComponent(engineType, c1Port, component1, dbConnString);

                var finishedEvent = new AutoResetEvent(false);
                var registeredEvent = new AutoResetEvent(false);
                component1.RegisterService(1, registeredEvent, finishedEvent);
                registeredEvent.WaitOne(TimeSpan.FromSeconds(10));

                var target = component1.GetServiceProxy<ITestService1>();
                var actual = target.ReturnMethod();
                finishedEvent.WaitOne(TimeSpan.FromSeconds(5));

                Assert.IsTrue(component1.Tracker.ReturnMethodCalled == 1);
                Assert.IsTrue(component1.Tracker.ParametersLastCall.Count == 0);
                Assert.AreEqual(component1.Tracker.ResultLastCall, actual);
            }
        }

        [Test,TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void CanInvoke_LocalCustomService_OneParamMethod_ReturnValue(DbEngineType engineType)
        {
            string dbConnString = TestSettingsProvider.GetConnString(engineType);

            var c1Port = new TestPort(10000);
            
            using (var component1 = TestComponent.GetComponent(true))
            {
                InitializeLonelyComponent(engineType, c1Port, component1, dbConnString);

                var finishedEvent = new AutoResetEvent(false);
                var registeredEvent = new AutoResetEvent(false);
                component1.RegisterService(1, registeredEvent, finishedEvent);
                registeredEvent.WaitOne(TimeSpan.FromSeconds(10));

                var target = component1.GetServiceProxy<ITestService1>();
                var expected = new AcceptanceMessageType1();

                var actual = target.ReturnMethodWithOneParameter(expected);
                finishedEvent.WaitOne(TimeSpan.FromSeconds(5));

                Assert.IsTrue(component1.Tracker.ReturnMethodWithOneParameterCalled == 1);
                Assert.IsTrue(component1.Tracker.ParametersLastCall.Count == 1);
                Assert.AreEqual(expected, component1.Tracker.ParametersLastCall[0]);
                Assert.AreEqual(component1.Tracker.ResultLastCall, actual);
            }
        }

        [Test,TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void CanInvoke_LocalCustomService_SeveralParamsMethod_ReturnValue(DbEngineType engineType)
        {
            string dbConnString = TestSettingsProvider.GetConnString(engineType);

            var c1Port = new TestPort(10000);
            
            using (var component1 = TestComponent.GetComponent(true))
            {
                InitializeLonelyComponent(engineType, c1Port, component1, dbConnString);

                var finishedEvent = new AutoResetEvent(false);
                var registeredEvent = new AutoResetEvent(false);
                component1.RegisterService(1, registeredEvent, finishedEvent);
                registeredEvent.WaitOne(TimeSpan.FromSeconds(10));

                var target = component1.GetServiceProxy<ITestService1>();
                var expected1 = new AcceptanceMessageType1();
                var expected2 = new AcceptanceMessageType2();

                var actual = target.ReturnMethodWithSeveralParameters(expected1, expected2);
                finishedEvent.WaitOne(TimeSpan.FromSeconds(5));

                Assert.IsTrue(component1.Tracker.ReturnMethodWithSeveralParametersCalled == 1);
                Assert.IsTrue(component1.Tracker.ParametersLastCall.Count == 2);
                Assert.AreEqual(expected1, component1.Tracker.ParametersLastCall[0]);
                Assert.AreEqual(expected2, component1.Tracker.ParametersLastCall[1]);
                Assert.AreEqual(component1.Tracker.ResultLastCall, actual);
            }
        }

        
        [Test,TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void CanInvoke_LocalCustomService_ArrayParamsMethod_Returns(DbEngineType engineType)
        {
            string dbConnString = TestSettingsProvider.GetConnString(engineType);

            var c1Port = new TestPort(10000);
            
            using (var component1 = TestComponent.GetComponent(true))
            {
                InitializeLonelyComponent(engineType, c1Port, component1, dbConnString);

                var finishedEvent = new AutoResetEvent(false);
                var registeredEvent = new AutoResetEvent(false);
                component1.RegisterService(1, registeredEvent, finishedEvent);
                registeredEvent.WaitOne(TimeSpan.FromSeconds(10));

                var target = component1.GetServiceProxy<ITestService1>();
                var expected1 = new AcceptanceMessageType1();
                var expected2 = new AcceptanceMessageType1();

                var actual = target.ReturnMethodWithSeveralParametersParams(expected1, expected2);
                finishedEvent.WaitOne(TimeSpan.FromSeconds(5));

                Assert.IsTrue(component1.Tracker.ReturnMethodWithArrayParametersCalled == 1);
                Assert.IsTrue(component1.Tracker.ParametersLastCall.Count == 2);
                Assert.AreEqual(expected1, component1.Tracker.ParametersLastCall[0]);
                Assert.AreEqual(expected2, component1.Tracker.ParametersLastCall[1]);
                Assert.AreEqual(component1.Tracker.ResultLastCall, actual);
            }
        }
       
        [Test,TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void CanInvoke_LocalCustomService_ReturnsArray(DbEngineType engineType)
        {
            string dbConnString = TestSettingsProvider.GetConnString(engineType);

            var c1Port = new TestPort(10000);
            
            using (var component1 = TestComponent.GetComponent(true))
            {
                InitializeLonelyComponent(engineType, c1Port, component1, dbConnString);

                var finishedEvent = new AutoResetEvent(false);
                var registeredEvent = new AutoResetEvent(false);
                component1.RegisterService(1, registeredEvent, finishedEvent);
                registeredEvent.WaitOne(TimeSpan.FromSeconds(10));

                var target = component1.GetServiceProxy<ITestService1>();
                var actual = target.ReturnArrayMethod();
                finishedEvent.WaitOne(TimeSpan.FromSeconds(5));

                Assert.IsTrue(component1.Tracker.ReturnArrayMethodCalled == 1);
                Assert.IsTrue(component1.Tracker.ParametersLastCall.Count == 0);
                CollectionAssert.AreEqual((AcceptanceMessageType1[])component1.Tracker.ResultLastCall, actual);
            }
        }

        #endregion

        #region remote

        [Test,TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void CanInvoke_RemoteCustomService_EmptyMethod(DbEngineType engineType)
        {
            string dbConnString = TestSettingsProvider.GetConnString(engineType);

            var c1Port = new TestPort(10000);
            var c2Port = new TestPort(11000);
            
            using (var component1 = TestComponent.GetComponent())
            using (var component2 = TestComponent.GetComponent(true))
            {
                InitializeLonelyComponent(engineType,c1Port, component1, dbConnString);
                InitializeConnectedComponent(engineType, c1Port, c2Port, component1, dbConnString, component2);

                var finishedEvent = new AutoResetEvent(false);
                var registeredEvent = new AutoResetEvent(false);
                component1.RegisterService(1, registeredEvent, finishedEvent);
                registeredEvent.WaitOne(TimeSpan.FromSeconds(10));

                var target = component2.GetServiceProxy<ITestService1>();
                target.EmptyMethod();
                finishedEvent.WaitOne(TimeSpan.FromSeconds(5));

                Assert.IsTrue(component2.Tracker.EmptyMethodCalled == 1);
                Assert.IsTrue(component2.Tracker.ParametersLastCall.Count == 0);
            }
        }

        [Test,TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void CanInvoke_RemoteCustomService_OneParamMethod(DbEngineType engineType)
        {
            string dbConnString = TestSettingsProvider.GetConnString(engineType);

            var c1Port = new TestPort(10000);
            var c2Port = new TestPort(11000);
            
            using (var component1 = TestComponent.GetComponent())
            using (var component2 = TestComponent.GetComponent(true))
            {
                InitializeLonelyComponent(engineType, c1Port, component1, dbConnString);
                InitializeConnectedComponent(engineType, c1Port, c2Port, component1, dbConnString, component2);

                var finishedEvent = new AutoResetEvent(false);
                var registeredEvent = new AutoResetEvent(false);
                component1.RegisterService(1, registeredEvent, finishedEvent);
                registeredEvent.WaitOne(TimeSpan.FromSeconds(10));

                var target = component2.GetServiceProxy<ITestService1>();
                var expected = new AcceptanceMessageType1();

                target.EmptyMethodWithOneParameter(expected);
                finishedEvent.WaitOne(TimeSpan.FromSeconds(5));

                Assert.IsTrue(component2.Tracker.EmptyMethodWithOneParameterCalled == 1);
                Assert.IsTrue(component2.Tracker.ParametersLastCall.Count == 1);
                Assert.AreEqual(expected, component2.Tracker.ParametersLastCall[0]);
            }
        }

        [Test,TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void CanInvoke_RemoteCustomService_SeveralParamsMethod(DbEngineType engineType)
        {
            string dbConnString = TestSettingsProvider.GetConnString(engineType);

            var c1Port = new TestPort(10000);
            var c2Port = new TestPort(11000);
            
            using (var component1 = TestComponent.GetComponent())
            using (var component2 = TestComponent.GetComponent(true))
            {
                InitializeLonelyComponent(engineType, c1Port, component1, dbConnString);
                InitializeConnectedComponent(engineType, c1Port, c2Port, component1, dbConnString, component2);

                var finishedEvent = new AutoResetEvent(false);
                var registeredEvent = new AutoResetEvent(false);
                component1.RegisterService(1, registeredEvent, finishedEvent);
                registeredEvent.WaitOne(TimeSpan.FromSeconds(10));

                var target = component2.GetServiceProxy<ITestService1>();
                var expected1 = new AcceptanceMessageType1();
                var expected2 = new AcceptanceMessageType2();

                target.EmptyMethodWithSeveralParameters(expected1, expected2);
                finishedEvent.WaitOne(TimeSpan.FromSeconds(5));

                Assert.IsTrue(component2.Tracker.EmptyMethodWithSeveralParametersCalled == 1);
                Assert.IsTrue(component2.Tracker.ParametersLastCall.Count == 2);
                Assert.AreEqual(expected1, component2.Tracker.ParametersLastCall[0]);
                Assert.AreEqual(expected2, component2.Tracker.ParametersLastCall[1]);
            }
        }


        [Test,TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void CanInvoke_RemoteCustomService_EmptyMethod_ReturnValue(DbEngineType engineType)
        {
            string dbConnString = TestSettingsProvider.GetConnString(engineType);

            var c1Port = new TestPort(10000);
            var c2Port = new TestPort(11000);
            
            using (var component1 = TestComponent.GetComponent())
            using (var component2 = TestComponent.GetComponent(true))
            {
                InitializeLonelyComponent(engineType, c1Port, component1, dbConnString);
                InitializeConnectedComponent(engineType, c1Port, c2Port, component1, dbConnString, component2);

                var finishedEvent = new AutoResetEvent(false);
                var registeredEvent = new AutoResetEvent(false);
                component1.RegisterService(1, registeredEvent, finishedEvent);
                registeredEvent.WaitOne(TimeSpan.FromSeconds(10));

                var target = component2.GetServiceProxy<ITestService1>();
                var actual = target.ReturnMethod();
                finishedEvent.WaitOne(TimeSpan.FromSeconds(5));

                Assert.IsTrue(component2.Tracker.ReturnMethodCalled == 1);
                Assert.IsTrue(component2.Tracker.ParametersLastCall.Count == 0);
                Assert.AreEqual(component2.Tracker.ResultLastCall, actual);
            }
        }

        [Test,TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void CanInvoke_RemoteCustomService_OneParamMethod_ReturnValue(DbEngineType engineType)
        {
            string dbConnString = TestSettingsProvider.GetConnString(engineType);

            var c1Port = new TestPort(10000);
            var c2Port = new TestPort(11000);
            
            using (var component1 = TestComponent.GetComponent())
            using (var component2 = TestComponent.GetComponent(true))
            {
                InitializeLonelyComponent(engineType, c1Port, component1, dbConnString);
                InitializeConnectedComponent(engineType, c1Port, c2Port, component1, dbConnString, component2);

                var finishedEvent = new AutoResetEvent(false);
                var registeredEvent = new AutoResetEvent(false);
                component1.RegisterService(1, registeredEvent, finishedEvent);
                registeredEvent.WaitOne(TimeSpan.FromSeconds(10));

                var target = component2.GetServiceProxy<ITestService1>();
                var expected = new AcceptanceMessageType1();

                var actual = target.ReturnMethodWithOneParameter(expected);
                finishedEvent.WaitOne(TimeSpan.FromSeconds(5));

                Assert.IsTrue(component2.Tracker.ReturnMethodWithOneParameterCalled == 1);
                Assert.IsTrue(component2.Tracker.ParametersLastCall.Count == 1);
                Assert.AreEqual(expected, component2.Tracker.ParametersLastCall[0]);
                Assert.AreEqual(component2.Tracker.ResultLastCall, actual);
            }
        }

        [Test,TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void CanInvoke_RemoteCustomService_SeveralParamsMethod_ReturnValue(DbEngineType engineType)
        {
            string dbConnString = TestSettingsProvider.GetConnString(engineType);

            var c1Port = new TestPort(10000);
            var c2Port = new TestPort(11000);
            
            using (var component1 = TestComponent.GetComponent())
            using (var component2 = TestComponent.GetComponent(true))
            {
                InitializeLonelyComponent(engineType, c1Port, component1, dbConnString);
                InitializeConnectedComponent(engineType, c1Port, c2Port, component1, dbConnString, component2);

                var finishedEvent = new AutoResetEvent(false);
                var registeredEvent = new AutoResetEvent(false);
                component1.RegisterService(1, registeredEvent, finishedEvent);
                registeredEvent.WaitOne(TimeSpan.FromSeconds(10));

                var target = component2.GetServiceProxy<ITestService1>();
                var expected1 = new AcceptanceMessageType1();
                var expected2 = new AcceptanceMessageType2();

                var actual = target.ReturnMethodWithSeveralParameters(expected1, expected2);
                finishedEvent.WaitOne(TimeSpan.FromSeconds(5));

                Assert.IsTrue(component2.Tracker.ReturnMethodWithSeveralParametersCalled == 1);
                Assert.IsTrue(component2.Tracker.ParametersLastCall.Count == 2);
                Assert.AreEqual(expected1, component2.Tracker.ParametersLastCall[0]);
                Assert.AreEqual(expected2, component2.Tracker.ParametersLastCall[1]);
                Assert.AreEqual(component2.Tracker.ResultLastCall, actual);
            }
        }

        [Test,TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void CanInvoke_RemoteCustomService_ArrayParamsMethod_Returns(DbEngineType engineType)
        {
            string dbConnString = TestSettingsProvider.GetConnString(engineType);

            var c1Port = new TestPort(10000);
            var c2Port = new TestPort(11000);

            using (var component1 = TestComponent.GetComponent())
            using (var component2 = TestComponent.GetComponent(true))
            {
                InitializeLonelyComponent(engineType, c1Port, component1, dbConnString);
                InitializeConnectedComponent(engineType, c1Port, c2Port, component1, dbConnString, component2);

                var finishedEvent = new AutoResetEvent(false);
                var registeredEvent = new AutoResetEvent(false);
                component1.RegisterService(1, registeredEvent, finishedEvent);
                registeredEvent.WaitOne(TimeSpan.FromSeconds(10));

                var target = component2.GetServiceProxy<ITestService1>();
                var expected1 = new AcceptanceMessageType1();
                var expected2 = new AcceptanceMessageType1();

                var actual = target.ReturnMethodWithSeveralParametersParams(expected1, expected2);
                finishedEvent.WaitOne(TimeSpan.FromSeconds(5));

                Assert.IsTrue(component2.Tracker.ReturnMethodWithArrayParametersCalled == 1);
                Assert.IsTrue(component2.Tracker.ParametersLastCall.Count == 2);
                Assert.AreEqual(expected1, component2.Tracker.ParametersLastCall[0]);
                Assert.AreEqual(expected2, component2.Tracker.ParametersLastCall[1]);
                Assert.AreEqual(component2.Tracker.ResultLastCall, actual);
            }
        }

        [Test,TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void CanInvoke_RemoteCustomService_ReturnsArray(DbEngineType engineType)
        {
            string dbConnString = TestSettingsProvider.GetConnString(engineType);

            var c1Port = new TestPort(10000);
            var c2Port = new TestPort(11000);

            using (var component1 = TestComponent.GetComponent()) 
            using (var component2 = TestComponent.GetComponent(true))
            {
                InitializeLonelyComponent(engineType, c1Port, component1, dbConnString);
                InitializeConnectedComponent(engineType, c1Port, c2Port, component1, dbConnString, component2);

                var finishedEvent = new AutoResetEvent(false);
                var registeredEvent = new AutoResetEvent(false);
                component1.RegisterService(1, registeredEvent, finishedEvent);
                registeredEvent.WaitOne(TimeSpan.FromSeconds(10));

                var target = component2.GetServiceProxy<ITestService1>();
                var actual = target.ReturnArrayMethod();
                finishedEvent.WaitOne(TimeSpan.FromSeconds(5));

                Assert.IsTrue(component2.Tracker.ReturnArrayMethodCalled == 1);
                Assert.IsTrue(component2.Tracker.ParametersLastCall.Count == 0);
                CollectionAssert.AreEqual((AcceptanceMessageType1[])component2.Tracker.ResultLastCall, actual);
            }
        }

        
        #endregion


        [Test,TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void When_Service_NotDefined_Returns_Null(DbEngineType engineType)
        {
            string dbConnString = TestSettingsProvider.GetConnString(engineType);

            ushort c1Port = new TestPort(9000);

            using (var component1 = TestComponent.GetComponent(true))            
            {
                InitializeLonelyComponent(engineType, c1Port, component1, dbConnString);
                
                var target = component1.GetServiceProxy<ITestService1>();
                Assert.IsNull(target);
            }
        }

        [Test,TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void When_CannotInvoke_ThrowsException(DbEngineType engineType)
        {
            string dbConnString = TestSettingsProvider.GetConnString(engineType);

            var c1Port = new TestPort(10000);
            var c2Port = new TestPort(11000);

            using (var component2 = TestComponent.GetComponent(true))
            {
                var finishedEvent = new AutoResetEvent(false);
                using (var component1 = TestComponent.GetComponent())
                {
                    InitializeLonelyComponent(engineType, c1Port, component1, dbConnString);
                    InitializeConnectedComponent(engineType, c1Port, c2Port, component1, dbConnString, component2);

                    var registeredEvent = new AutoResetEvent(false);
                    component1.RegisterService(1, registeredEvent, finishedEvent);
                    registeredEvent.WaitOne(TimeSpan.FromSeconds(10));
                }
                var target = component2.GetServiceProxy<ITestService1>();
                Assert.Throws<ermeXComponentNotAvailableException>(target.EmptyMethod);

                Assert.IsTrue(component2.Tracker.EmptyMethodCalled == 0);
                Assert.IsTrue(component2.Tracker.ParametersLastCall.Count == 0);
            }

        }

        [Test,TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void TwoComponents_CannotRegister_TheSameService_IfReturnValues(DbEngineType engineType)
        {
            string dbConnString = TestSettingsProvider.GetConnString(engineType);

            var c1Port = new TestPort(10000);
            var c2Port = new TestPort(11000);

            using (var component1 = TestComponent.GetComponent())
            using (var component2 = TestComponent.GetComponent(true))
            {
                InitializeLonelyComponent(engineType, c1Port, component1, dbConnString);
                InitializeConnectedComponent(engineType, c1Port, c2Port, component1, dbConnString, component2);

                var finishedEvent1 = new AutoResetEvent(false);
                var registeredEvent1 = new AutoResetEvent(false);
                var finishedEvent2 = new AutoResetEvent(false);
                var registeredEvent2 = new AutoResetEvent(false);
                
                component1.RegisterService<ITestService1>(1, registeredEvent1, finishedEvent1);
                Assert.Throws<InvalidOperationException>(()=>component2.RegisterService<ITestService1>(0, registeredEvent2, finishedEvent2));
            }
        }

        [Test,TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void TwoComponents_CanRegister_TheSameService_If_Dont_ReturnValues(DbEngineType engineType)
        {
            string dbConnString = TestSettingsProvider.GetConnString(engineType);

            var c1Port = new TestPort(10000);
            var c2Port = new TestPort(11000);

            using (var component1 = TestComponent.GetComponent())
            using (var component2 = TestComponent.GetComponent(true))
            {
                InitializeLonelyComponent(engineType, c1Port, component1, dbConnString);
                InitializeConnectedComponent(engineType, c1Port, c2Port, component1, dbConnString, component2);

                var finishedEvent1 = new AutoResetEvent(false);
                var registeredEvent1 = new AutoResetEvent(false);
                var finishedEvent2 = new AutoResetEvent(false);
                var registeredEvent2 = new AutoResetEvent(false);

                component1.RegisterService<ITestService3>(0, registeredEvent1, finishedEvent1);
                component2.RegisterService<ITestService3>(0, registeredEvent2, finishedEvent2);
            }
        }
      
        [Test,TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void Components_CanPublish_Several_Services(DbEngineType engineType)
        {
            string dbConnString = TestSettingsProvider.GetConnString(engineType);

            var c1Port = new TestPort(10000);
            var c2Port = new TestPort(11000);

            using (var component1 = TestComponent.GetComponent())
            using (var component2 = TestComponent.GetComponent(true))
            {
                InitializeLonelyComponent(engineType, c1Port, component1, dbConnString);
                InitializeConnectedComponent(engineType, c1Port, c2Port, component1, dbConnString, component2);

                var finishedEvent1 = new AutoResetEvent(false);
                var registeredEvent1 = new AutoResetEvent(false);
                component1.RegisterService<ITestService1>(1, registeredEvent1, finishedEvent1);

                var finishedEvent2 = new AutoResetEvent(false);
                var registeredEvent2 = new AutoResetEvent(false);
                component1.RegisterService<ITestService2>(1, registeredEvent2, finishedEvent2);

                WaitHandle.WaitAll(new []{registeredEvent1,registeredEvent2},TimeSpan.FromSeconds(10));

                var target1 = component2.GetServiceProxy<ITestService1>();
                var target2 = component2.GetServiceProxy<ITestService2>();

                target1.EmptyMethod();

                Assert.IsTrue(component2.Tracker.EmptyMethodCalled == 1);
                Assert.IsTrue(component2.Tracker.ParametersLastCall.Count == 0);

                target2.EmptyMethod();

                WaitHandle.WaitAll(new[] { finishedEvent1, finishedEvent2 }, TimeSpan.FromSeconds(10));

                Assert.IsTrue(component2.Tracker.EmptyMethodCalled == 2, string.Format("component2.Tracker.EmptyMethodCalled: {0}", component2.Tracker.EmptyMethodCalled));
                Assert.IsTrue(component2.Tracker.ParametersLastCall.Count == 0);

               
            }
        }

        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void SeveralComponents_Can_Serve_Same_Service_NoReturnValues(DbEngineType engineType)
        {
            string dbConnString = TestSettingsProvider.GetConnString(engineType);

            var c1Port = new TestPort(10000);
            var c2Port = new TestPort(11000);
            var c3Port = new TestPort(12000);

            var c1Id = new Guid("E80C1A32-990F-42E7-AF19-78E90BCF1CFA");
            var c2Id = new Guid("CA100CC0-35D4-44A2-A29C-C966A4F1F3EF");
            var c3Id = new Guid("44087493-E076-438A-B513-15FDBAA0AAF1");

            using (var component1 = TestComponent.GetComponent(c1Id))
            using (var component2 = TestComponent.GetComponent(c2Id,true))
            using (var component3 = TestComponent.GetComponent(c3Id))
            {
                InitializeLonelyComponent(engineType, c1Port, component1, dbConnString);
                InitializeConnectedComponent(engineType, c1Port, c2Port, component1, dbConnString, component2);
                InitializeConnectedComponent(engineType, c1Port, c3Port, component1, dbConnString, component3);

                var finishedEvent1 = new AutoResetEvent(false);
                var registeredEvent1 = new AutoResetEvent(false);
                var finishedEvent2 = new AutoResetEvent(false);
                var registeredEvent2 = new AutoResetEvent(false);

                component2.RegisterService<ITestService3>(1, registeredEvent1, finishedEvent1);
                component3.RegisterService<ITestService3>(1, registeredEvent2, finishedEvent2);
                WaitHandle.WaitAll(new[] { registeredEvent1, registeredEvent2}, TimeSpan.FromSeconds(20));
               
                var serviceProxy = component2.GetServiceProxy<ITestService3>();

                serviceProxy.EmptyMethod();

                WaitHandle.WaitAll(new[] { finishedEvent1, finishedEvent2 }, TimeSpan.FromSeconds(20));

                Assert.IsTrue(component2.Tracker.EmptyMethodCalled == 1, string.Format("component2.Tracker.EmptyMethodCalled: {0}", component2.Tracker.EmptyMethodCalled));
                Assert.IsTrue(component2.Tracker.ParametersLastCall.Count == 0);
                
                Assert.IsTrue(component3.Tracker.EmptyMethodCalled == 1, string.Format("component3.Tracker.EmptyMethodCalled: {0}", component3.Tracker.EmptyMethodCalled));
                Assert.IsTrue(component3.Tracker.ParametersLastCall.Count == 0);
            }

            c1Port.Dispose();
            c2Port.Dispose();
            c3Port.Dispose();
        }

   }
}
